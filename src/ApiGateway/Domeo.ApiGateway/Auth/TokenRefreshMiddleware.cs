using System.Text;
using System.Text.Json;
using Auth.Contracts.DTOs;
using Auth.Contracts.Routes;
using Domeo.Shared.Contracts;

namespace Domeo.ApiGateway.Auth;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TokenRefreshMiddleware> _logger;

    // Prevent concurrent refresh race condition:
    // multiple parallel requests with same expired token should share one refresh call
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private volatile CachedRefreshResult? _cachedResult;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TokenRefreshMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory,
        ILogger<TokenRefreshMiddleware> logger)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip only auth endpoints that don't need a token or could cause a loop.
        // /api/auth/me and /api/auth/token DO need a valid token → must NOT be skipped.
        var path = context.Request.Path;
        if (path.StartsWithSegments("/api/auth/login")
            || path.StartsWithSegments("/api/auth/callback")
            || path.StartsWithSegments("/api/auth/refresh")
            || path.StartsWithSegments("/api/auth/logout"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Cookies.TryGetValue(AuthRoutes.Cookies.AccessToken, out var accessToken)
            || string.IsNullOrEmpty(accessToken))
        {
            await _next(context);
            return;
        }

        if (!IsTokenExpiredOrExpiring(accessToken))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Cookies.TryGetValue(AuthRoutes.Cookies.RefreshToken, out var refreshToken)
            || string.IsNullOrEmpty(refreshToken))
        {
            await _next(context);
            return;
        }

        _logger.LogInformation("Access token expired or expiring, attempting refresh for {Path}", context.Request.Path);

        try
        {
            var newTokens = await RefreshWithDeduplicationAsync(refreshToken, context.RequestAborted);

            if (newTokens is not null)
            {
                context.Items["RefreshedAccessToken"] = newTokens.Token.AccessToken;

                SetCookie(context, AuthRoutes.Cookies.AccessToken,
                    newTokens.Token.AccessToken, newTokens.Token.ExpiresAt);
                SetCookie(context, AuthRoutes.Cookies.RefreshToken,
                    newTokens.Token.RefreshToken, DateTime.UtcNow.AddDays(7));

                _logger.LogInformation("Token refreshed successfully for user {UserId}", newTokens.User.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token refresh failed, proceeding with expired token");
        }

        await _next(context);
    }

    /// <summary>
    /// Ensures only one refresh call happens at a time.
    /// Concurrent requests with the same expired token reuse the cached result.
    /// </summary>
    private async Task<AuthResultDto?> RefreshWithDeduplicationAsync(string refreshToken, CancellationToken ct)
    {
        // Check if we already have a fresh cached result (from a concurrent request)
        var cached = _cachedResult;
        if (cached is not null
            && cached.RefreshToken == refreshToken
            && cached.Timestamp > DateTime.UtcNow.AddSeconds(-10))
        {
            _logger.LogDebug("Using cached refresh result");
            return cached.Result;
        }

        await _refreshLock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock — another thread may have refreshed
            cached = _cachedResult;
            if (cached is not null
                && cached.RefreshToken == refreshToken
                && cached.Timestamp > DateTime.UtcNow.AddSeconds(-10))
            {
                _logger.LogDebug("Using cached refresh result (after lock)");
                return cached.Result;
            }

            var result = await RefreshTokensAsync(refreshToken, ct);

            _cachedResult = new CachedRefreshResult(refreshToken, result, DateTime.UtcNow);

            return result;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static bool IsTokenExpiredOrExpiring(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            // Decode the payload (second part) without signature validation
            var payload = parts[1];

            // Fix base64url padding
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("exp", out var expElement))
                return false;

            var exp = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
            var now = DateTimeOffset.UtcNow;

            // Token is expired or expires within 30 seconds
            return exp <= now.AddSeconds(30);
        }
        catch
        {
            // Can't parse token — let the auth pipeline handle it
            return false;
        }
    }

    private async Task<AuthResultDto?> RefreshTokensAsync(string refreshToken, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("auth-internal");
        var request = new RefreshTokenRequest(refreshToken);
        var content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync($"/{AuthRoutes.Controller.Base}/{AuthRoutes.Controller.Refresh}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Auth service returned {StatusCode} on token refresh", response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResultDto>>(body, JsonOptions);

        if (result is not { Success: true, Data: not null })
        {
            _logger.LogWarning("Token refresh response was not successful: {Body}", body);
            return null;
        }

        return result.Data;
    }

    private static void SetCookie(HttpContext context, string name, string value, DateTime expires)
    {
        var isHttps = context.Request.IsHttps
                      || context.Request.Headers["X-Forwarded-Proto"] == "https";

        context.Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/",
            Expires = expires
        });
    }

    private sealed record CachedRefreshResult(string RefreshToken, AuthResultDto? Result, DateTime Timestamp);
}

public static class TokenRefreshMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TokenRefreshMiddleware>();
    }
}
