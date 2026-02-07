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
        // Skip auth endpoints to avoid refresh loops
        if (context.Request.Path.StartsWithSegments("/api/auth"))
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

        _logger.LogDebug("Access token expired, attempting refresh");

        try
        {
            var newTokens = await RefreshTokensAsync(refreshToken, context.RequestAborted);

            if (newTokens is not null)
            {
                context.Items["RefreshedAccessToken"] = newTokens.Token.AccessToken;

                SetCookie(context.Response, AuthRoutes.Cookies.AccessToken,
                    newTokens.Token.AccessToken, newTokens.Token.ExpiresAt);
                SetCookie(context.Response, AuthRoutes.Cookies.RefreshToken,
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
            _logger.LogWarning("Token refresh response was not successful");
            return null;
        }

        return result.Data;
    }

    private static void SetCookie(HttpResponse response, string name, string value, DateTime expires)
    {
        response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = expires
        });
    }
}

public static class TokenRefreshMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TokenRefreshMiddleware>();
    }
}
