using System.IdentityModel.Tokens.Jwt;
using Auth.API.Contracts;
using Auth.API.Entities;
using Auth.API.Persistence;
using Auth.API.Services;
using Domeo.Shared.Auth;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Contracts.Events;
using Domeo.Shared.Infrastructure.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Endpoints;

public static class AuthEndpoints
{
    private const string AccessTokenCookieName = "access_token";
    private const string RefreshTokenCookieName = "refresh_token";

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        // OAuth flow (browser-based)
        group.MapGet("/login", Login);
        group.MapGet("/callback", CallbackGet);

        // OAuth callback - API version for SPA/mobile
        group.MapPost("/callback", Callback);

        // Token management
        group.MapPost("/refresh", RefreshTokenEndpoint);
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
        group.MapGet("/token", GetToken);
    }

    /// <summary>
    /// Initiate OAuth login - redirects to Auth Center
    /// </summary>
    private static IResult Login(
        HttpContext httpContext,
        IConfiguration configuration,
        [FromQuery] string? returnUrl)
    {
        var authCenterUrl = configuration["AuthCenter:BaseUrl"] ?? "http://localhost:5100";
        var clientId = configuration["AuthCenter:ClientId"] ?? "domeo";
        var callbackUrl = configuration["AuthCenter:CallbackUrl"] ?? "http://localhost:5000/api/auth/callback";

        // Generate state for CSRF protection (store returnUrl in state)
        var state = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(
                System.Text.Json.JsonSerializer.Serialize(new { returnUrl = returnUrl ?? "/" })));

        var authorizeUrl = $"{authCenterUrl}/authorize?" +
            $"response_type=code&" +
            $"client_id={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
            $"state={Uri.EscapeDataString(state)}";

        return Results.Redirect(authorizeUrl);
    }

    /// <summary>
    /// OAuth callback (GET) - receives code from Auth Center, exchanges for tokens, redirects to frontend
    /// </summary>
    private static async Task<IResult> CallbackGet(
        HttpContext httpContext,
        [FromQuery] string code,
        [FromQuery] string? state,
        IAuthCenterClient authCenterClient,
        IEventPublisher eventPublisher,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var postLoginRedirect = configuration["Frontend:PostLoginRedirect"] ?? "/";
        var callbackUrl = configuration["AuthCenter:CallbackUrl"] ?? "http://localhost:5000/api/auth/callback";

        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("CallbackGet: code={Code}, frontendUrl={FrontendUrl}, callbackUrl={CallbackUrl}",
            code, frontendUrl, callbackUrl);

        // Parse returnUrl from state
        var returnUrl = postLoginRedirect;
        if (!string.IsNullOrEmpty(state))
        {
            try
            {
                var stateJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
                var stateObj = System.Text.Json.JsonSerializer.Deserialize<StatePayload>(stateJson);
                if (!string.IsNullOrEmpty(stateObj?.returnUrl))
                    returnUrl = stateObj.returnUrl;
            }
            catch { /* ignore invalid state */ }
        }

        // Exchange code for tokens at Auth Center
        var tokenResponse = await authCenterClient.ExchangeCodeAsync(code, callbackUrl, cancellationToken);
        if (tokenResponse is null)
        {
            return Results.Redirect($"{frontendUrl}/login?error=invalid_code");
        }

        // Parse JWT to extract user info
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenResponse.AccessToken);

        var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "viewer";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Results.Redirect($"{frontendUrl}/login?error=invalid_token");
        }

        var userGuid = Guid.Parse(userId);

        // Generate login session ID and publish event to Redis
        var ipAddress = GetClientIpAddress(httpContext);
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var loginSessionId = Guid.NewGuid();

        var loginEvent = new UserLoggedInEvent
        {
            UserId = userGuid,
            UserEmail = email,
            UserName = name,
            UserRole = role,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            SessionId = loginSessionId
        };
        await eventPublisher.PublishSessionAsync(loginEvent, cancellationToken);

        // Store Auth Center refresh token in our DB
        var refreshToken = RefreshToken.Create(
            userGuid,
            tokenResponse.RefreshToken,
            TimeSpan.FromDays(7),
            loginSessionId);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Set cookies
        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        SetAuthCookies(
            httpContext,
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            accessTokenExpiration,
            DateTime.UtcNow.AddDays(7));

        // Redirect to frontend
        var redirectTo = $"{frontendUrl}{returnUrl}";
        logger.LogInformation("CallbackGet: SUCCESS! Redirecting to {RedirectTo}", redirectTo);
        return Results.Redirect(redirectTo);
    }

    private sealed record StatePayload(string? returnUrl);

    private static bool IsBrowserClient(HttpRequest request)
    {
        var userAgent = request.Headers.UserAgent.ToString();
        var clientType = request.Headers["X-Client-Type"].ToString().ToLower();

        if (clientType == "mobile")
            return false;

        return userAgent.Contains("Mozilla") || userAgent.Contains("Chrome") || userAgent.Contains("Safari");
    }

    private static void SetAuthCookies(HttpContext httpContext, string accessToken, string refreshToken, DateTime accessTokenExpires, DateTime refreshTokenExpires)
    {
        // In development (HTTP), use Lax + non-Secure
        // In production (HTTPS), use None + Secure for cross-origin
        var isHttps = httpContext.Request.IsHttps ||
                      httpContext.Request.Headers["X-Forwarded-Proto"] == "https";

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = accessTokenExpires
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = refreshTokenExpires
        };

        httpContext.Response.Cookies.Append(AccessTokenCookieName, accessToken, accessCookieOptions);
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshCookieOptions);
    }

    private static void ClearAuthCookies(HttpContext httpContext)
    {
        var isHttps = httpContext.Request.IsHttps ||
                      httpContext.Request.Headers["X-Forwarded-Proto"] == "https";

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        httpContext.Response.Cookies.Delete(AccessTokenCookieName, cookieOptions);
        httpContext.Response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);
    }

    private static string? GetClientIpAddress(HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// OAuth callback - exchange authorization code for tokens from external Auth Center
    /// </summary>
    private static async Task<IResult> Callback(
        HttpContext httpContext,
        [FromBody] CallbackRequest request,
        IAuthCenterClient authCenterClient,
        IEventPublisher eventPublisher,
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Exchange code for tokens at Auth Center
        var tokenResponse = await authCenterClient.ExchangeCodeAsync(request.Code, request.RedirectUri, cancellationToken);
        if (tokenResponse is null)
        {
            return Results.Ok(ApiResponse<SsoAuthResultDto>.Fail("Invalid authorization code"));
        }

        // Parse JWT to extract user info
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenResponse.AccessToken);

        var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "viewer";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Results.Ok(ApiResponse<SsoAuthResultDto>.Fail("Invalid token claims"));
        }

        var userGuid = Guid.Parse(userId);

        // Generate login session ID and publish event to Redis
        var ipAddress = GetClientIpAddress(httpContext);
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var loginSessionId = Guid.NewGuid();

        var loginEvent = new UserLoggedInEvent
        {
            UserId = userGuid,
            UserEmail = email,
            UserName = name,
            UserRole = role,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            SessionId = loginSessionId
        };
        await eventPublisher.PublishSessionAsync(loginEvent, cancellationToken);

        // Store Auth Center refresh token in our DB for token refresh
        var refreshToken = RefreshToken.Create(
            userGuid,
            tokenResponse.RefreshToken,
            TimeSpan.FromDays(7),
            loginSessionId);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        // Set cookies for browser clients
        if (IsBrowserClient(httpContext.Request))
        {
            SetAuthCookies(
                httpContext,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                accessTokenExpiration,
                DateTime.UtcNow.AddDays(7));
        }

        // Build user info from JWT claims
        var nameParts = (name ?? email.Split('@')[0]).Split(' ', 2);
        var user = new SsoUserDto(
            userGuid,
            email,
            nameParts[0],
            nameParts.Length > 1 ? nameParts[1] : "",
            role);

        var tokenDto = new TokenDto(tokenResponse.AccessToken, tokenResponse.RefreshToken, accessTokenExpiration);

        return Results.Ok(ApiResponse<SsoAuthResultDto>.Ok(new SsoAuthResultDto(user, tokenDto)));
    }

    /// <summary>
    /// Refresh access token using Auth Center
    /// </summary>
    private static async Task<IResult> RefreshTokenEndpoint(
        HttpContext httpContext,
        [FromBody] RefreshTokenRequest? request,
        IAuthCenterClient authCenterClient,
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Try to get refresh token from request body or cookie
        var refreshTokenValue = request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshTokenValue);
        }

        if (string.IsNullOrEmpty(refreshTokenValue))
            return Results.Ok(ApiResponse<SsoAuthResultDto>.Fail("Refresh token is required"));

        // Verify token exists in our DB
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshTokenValue, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            return Results.Ok(ApiResponse<SsoAuthResultDto>.Fail("Invalid or expired refresh token"));

        // Refresh token at Auth Center
        var tokenResponse = await authCenterClient.RefreshTokenAsync(refreshTokenValue, cancellationToken);
        if (tokenResponse is null)
        {
            // Token was rejected by Auth Center, revoke locally
            storedToken.Revoke();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(ApiResponse<SsoAuthResultDto>.Fail("Failed to refresh token"));
        }

        // Revoke old token and store new one (preserve login session)
        storedToken.Revoke();

        var newRefreshToken = RefreshToken.Create(
            storedToken.UserId,
            tokenResponse.RefreshToken,
            TimeSpan.FromDays(7),
            storedToken.LoginSessionId);

        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Parse new JWT for user info
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenResponse.AccessToken);

        var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "viewer";

        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        // Set cookies for browser clients
        if (IsBrowserClient(httpContext.Request))
        {
            SetAuthCookies(
                httpContext,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                accessTokenExpiration,
                DateTime.UtcNow.AddDays(7));
        }

        var nameParts = (name ?? email?.Split('@')[0] ?? "").Split(' ', 2);
        var user = new SsoUserDto(
            Guid.Parse(userId ?? Guid.Empty.ToString()),
            email ?? "",
            nameParts[0],
            nameParts.Length > 1 ? nameParts[1] : "",
            role);

        var tokenDto = new TokenDto(tokenResponse.AccessToken, tokenResponse.RefreshToken, accessTokenExpiration);

        return Results.Ok(ApiResponse<SsoAuthResultDto>.Ok(new SsoAuthResultDto(user, tokenDto)));
    }

    /// <summary>
    /// Logout - revoke refresh token and record logout in audit
    /// </summary>
    private static async Task<IResult> Logout(
        HttpContext httpContext,
        [FromBody] LogoutRequest? request,
        ICurrentUserAccessor currentUserAccessor,
        IAuthCenterClient authCenterClient,
        IEventPublisher eventPublisher,
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user?.Id is null)
            return Results.Ok(ApiResponse.Fail("Unauthorized"));

        var userId = user.Id.Value;

        // Try to get refresh token from request body or cookie
        var refreshTokenValue = request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshTokenValue);
        }

        Guid? loginSessionId = null;

        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            // Revoke at Auth Center
            await authCenterClient.RevokeTokenAsync(refreshTokenValue, cancellationToken);

            // Revoke locally
            var refreshToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshTokenValue && x.UserId == userId, cancellationToken);

            if (refreshToken is not null)
            {
                loginSessionId = refreshToken.LoginSessionId;
                refreshToken.Revoke();
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        // Publish logout event to Redis
        if (loginSessionId.HasValue)
        {
            var logoutEvent = new UserLoggedOutEvent
            {
                UserId = userId,
                UserEmail = user.Email,
                UserName = user.Name,
                UserRole = user.Role,
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
                SessionId = loginSessionId.Value
            };
            await eventPublisher.PublishSessionAsync(logoutEvent, cancellationToken);
        }

        // Clear cookies
        ClearAuthCookies(httpContext);

        return Results.Ok(ApiResponse.Ok("Logged out successfully"));
    }

    /// <summary>
    /// Get current user info from JWT token
    /// </summary>
    private static IResult GetCurrentUser(ICurrentUserAccessor currentUserAccessor)
    {
        var user = currentUserAccessor.User;
        if (user is null)
            return Results.Ok(ApiResponse.Fail("Unauthorized"));

        return Results.Ok(ApiResponse<object>.Ok(new
        {
            user.Id,
            user.Email,
            user.Name,
            user.Role,
            Roles = currentUserAccessor.Roles
        }));
    }

    /// <summary>
    /// Get JWT token from HttpOnly cookie for WebSocket authentication
    /// </summary>
    private static IResult GetToken(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(AccessTokenCookieName, out var token))
        {
            return Results.Ok(ApiResponse<object>.Ok(new { token }));
        }

        return Results.Ok(ApiResponse<object>.Fail("Not authenticated"));
    }
}

/// <summary>
/// User info extracted from SSO JWT token
/// </summary>
public sealed record SsoUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role);

/// <summary>
/// Auth result for SSO authentication
/// </summary>
public sealed record SsoAuthResultDto(
    SsoUserDto User,
    TokenDto Token);
