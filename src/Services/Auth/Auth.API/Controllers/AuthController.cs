using Auth.Application.Services;
using Auth.Contracts;
using Auth.Contracts.DTOs;
using Auth.Contracts.Routes;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Domeo.Shared.Application;
using Domeo.Shared.Contracts;
using Domeo.Shared.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route(AuthRoutes.Controller.Base)]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthCenterClient _authCenterClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IAuthCenterClient authCenterClient,
        IEventPublisher eventPublisher,
        ICurrentUserAccessor currentUserAccessor,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _authCenterClient = authCenterClient;
        _eventPublisher = eventPublisher;
        _currentUserAccessor = currentUserAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Initiate OAuth login - redirects to Auth Center
    /// </summary>
    [HttpGet(AuthRoutes.Controller.Login)]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        var authCenterUrl = _configuration["AuthCenter:BaseUrl"] ?? "http://localhost:5100";
        var clientId = _configuration["AuthCenter:ClientId"] ?? "domeo";
        var callbackUrl = _configuration["AuthCenter:CallbackUrl"] ?? "http://localhost:5000/api/auth/callback";

        var state = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(
                System.Text.Json.JsonSerializer.Serialize(new { returnUrl = returnUrl ?? "/" })));

        var authorizeUrl = $"{authCenterUrl}{AuthRoutes.External.AuthorizeEndpoint}?" +
            $"response_type=code&" +
            $"client_id={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
            $"state={Uri.EscapeDataString(state)}";

        return Redirect(authorizeUrl);
    }

    /// <summary>
    /// OAuth callback (GET) - receives code from Auth Center, exchanges for tokens, redirects to frontend
    /// </summary>
    [HttpGet(AuthRoutes.Controller.Callback)]
    public async Task<IActionResult> CallbackGet(
        [FromQuery] string code,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var postLoginRedirect = _configuration["Frontend:PostLoginRedirect"] ?? "/";
        var callbackUrl = _configuration["AuthCenter:CallbackUrl"] ?? "http://localhost:5000/api/auth/callback";

        _logger.LogInformation("CallbackGet: code={Code}, frontendUrl={FrontendUrl}, callbackUrl={CallbackUrl}",
            code, frontendUrl, callbackUrl);

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

        var tokenResponse = await _authCenterClient.ExchangeCodeAsync(code, callbackUrl, cancellationToken);
        if (tokenResponse is null)
        {
            return Redirect($"{frontendUrl}{AuthRoutes.External.FrontendLoginPath}?error=invalid_code");
        }

        if (string.IsNullOrEmpty(tokenResponse.UserId))
        {
            return Redirect($"{frontendUrl}{AuthRoutes.External.FrontendLoginPath}?error=invalid_token");
        }

        var userGuid = Guid.Parse(tokenResponse.UserId);

        var ipAddress = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        var loginSessionId = Guid.NewGuid();

        var loginEvent = new UserLoggedInEvent
        {
            UserId = userGuid,
            UserRole = tokenResponse.Role,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            SessionId = loginSessionId
        };
        await _eventPublisher.PublishSessionAsync(loginEvent, cancellationToken);

        var refreshToken = RefreshToken.Create(
            userGuid,
            tokenResponse.RefreshToken,
            TimeSpan.FromDays(7),
            loginSessionId);

        _refreshTokenRepository.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        SetAuthCookies(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            accessTokenExpiration,
            DateTime.UtcNow.AddDays(7));

        var redirectTo = $"{frontendUrl}{returnUrl}";
        _logger.LogInformation("CallbackGet: SUCCESS! Redirecting to {RedirectTo}", redirectTo);
        return Redirect(redirectTo);
    }

    /// <summary>
    /// OAuth callback - exchange authorization code for tokens from external Auth Center
    /// </summary>
    [HttpPost(AuthRoutes.Controller.Callback)]
    public async Task<IActionResult> Callback(
        [FromBody] CallbackRequest request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _authCenterClient.ExchangeCodeAsync(request.Code, request.RedirectUri, cancellationToken);
        if (tokenResponse is null)
        {
            return Ok(ApiResponse<AuthResultDto>.Fail("Invalid authorization code"));
        }

        if (string.IsNullOrEmpty(tokenResponse.UserId))
        {
            return Ok(ApiResponse<AuthResultDto>.Fail("Invalid token claims"));
        }

        var userGuid = Guid.Parse(tokenResponse.UserId);

        var ipAddress = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        var loginSessionId = Guid.NewGuid();

        var loginEvent = new UserLoggedInEvent
        {
            UserId = userGuid,
            UserRole = tokenResponse.Role,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            SessionId = loginSessionId
        };
        await _eventPublisher.PublishSessionAsync(loginEvent, cancellationToken);

        var refreshToken = RefreshToken.Create(
            userGuid,
            tokenResponse.RefreshToken,
            TimeSpan.FromDays(7),
            loginSessionId);

        _refreshTokenRepository.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        if (IsBrowserClient())
        {
            SetAuthCookies(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                accessTokenExpiration,
                DateTime.UtcNow.AddDays(7));
        }

        var userDto = new UserDto(userGuid, tokenResponse.Role);
        var tokenDto = new TokenDto(tokenResponse.AccessToken, tokenResponse.RefreshToken, accessTokenExpiration);

        return Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(userDto, tokenDto)));
    }

    /// <summary>
    /// Refresh access token using Auth Center
    /// </summary>
    [HttpPost(AuthRoutes.Controller.Refresh)]
    public async Task<IActionResult> RefreshTokenEndpoint(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        var refreshTokenValue = request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            Request.Cookies.TryGetValue(AuthRoutes.Cookies.RefreshToken, out refreshTokenValue);
        }

        if (string.IsNullOrEmpty(refreshTokenValue))
            return Ok(ApiResponse<AuthResultDto>.Fail("Refresh token is required"));

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            return Ok(ApiResponse<AuthResultDto>.Fail("Invalid or expired refresh token"));

        var tokenResponse = await _authCenterClient.RefreshTokenAsync(refreshTokenValue, cancellationToken);
        if (tokenResponse is null)
        {
            storedToken.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse<AuthResultDto>.Fail("Failed to refresh token"));
        }

        storedToken.Revoke();

        var newRefreshToken = RefreshToken.Create(
            storedToken.UserId,
            tokenResponse.RefreshToken,
            TimeSpan.FromDays(7),
            storedToken.LoginSessionId);

        _refreshTokenRepository.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        if (IsBrowserClient())
        {
            SetAuthCookies(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                accessTokenExpiration,
                DateTime.UtcNow.AddDays(7));
        }

        var userDto = new UserDto(storedToken.UserId, tokenResponse.Role);
        var tokenDto = new TokenDto(tokenResponse.AccessToken, tokenResponse.RefreshToken, accessTokenExpiration);

        return Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(userDto, tokenDto)));
    }

    /// <summary>
    /// Logout - revoke refresh token and record logout in audit
    /// </summary>
    [HttpPost(AuthRoutes.Controller.Logout)]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest? request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserAccessor.User;
        if (user?.Id is null)
            return Ok(ApiResponse.Fail("Unauthorized"));

        var userId = user.Id.Value;

        var refreshTokenValue = request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            Request.Cookies.TryGetValue(AuthRoutes.Cookies.RefreshToken, out refreshTokenValue);
        }

        Guid? loginSessionId = null;

        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            await _authCenterClient.RevokeTokenAsync(refreshTokenValue, cancellationToken);

            var refreshToken = await _refreshTokenRepository.GetByTokenAndUserIdAsync(
                refreshTokenValue, userId, cancellationToken);

            if (refreshToken is not null)
            {
                loginSessionId = refreshToken.LoginSessionId;
                refreshToken.Revoke();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        if (loginSessionId.HasValue)
        {
            var logoutEvent = new UserLoggedOutEvent
            {
                UserId = userId,
                UserRole = user.Role,
                IpAddress = GetClientIpAddress(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                SessionId = loginSessionId.Value
            };
            await _eventPublisher.PublishSessionAsync(logoutEvent, cancellationToken);
        }

        ClearAuthCookies();

        return Ok(ApiResponse.Ok("Logged out successfully"));
    }

    /// <summary>
    /// Get current user info from JWT token
    /// </summary>
    [HttpGet(AuthRoutes.Controller.Me)]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var user = _currentUserAccessor.User;
        if (user is null)
            return Ok(ApiResponse.Fail("Unauthorized"));

        return Ok(ApiResponse<UserDto>.Ok(new UserDto(user.Id!.Value, user.Role)));
    }

    /// <summary>
    /// Get JWT token from HttpOnly cookie for WebSocket authentication
    /// </summary>
    [HttpGet(AuthRoutes.Controller.Token)]
    public IActionResult GetToken()
    {
        if (Request.Cookies.TryGetValue(AuthRoutes.Cookies.AccessToken, out var token))
        {
            return Ok(ApiResponse<object>.Ok(new { token }));
        }

        return Ok(ApiResponse<object>.Fail("Not authenticated"));
    }

    #region Helper Methods

    private sealed record StatePayload(string? returnUrl);

    private bool IsBrowserClient()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var clientType = Request.Headers["X-Client-Type"].ToString().ToLower();

        if (clientType == "mobile")
            return false;

        return userAgent.Contains("Mozilla") || userAgent.Contains("Chrome") || userAgent.Contains("Safari");
    }

    private void SetAuthCookies(string accessToken, string refreshToken, DateTime accessTokenExpires, DateTime refreshTokenExpires)
    {
        var isHttps = Request.IsHttps || Request.Headers["X-Forwarded-Proto"] == "https";

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

        Response.Cookies.Append(AuthRoutes.Cookies.AccessToken, accessToken, accessCookieOptions);
        Response.Cookies.Append(AuthRoutes.Cookies.RefreshToken, refreshToken, refreshCookieOptions);
    }

    private void ClearAuthCookies()
    {
        var isHttps = Request.IsHttps || Request.Headers["X-Forwarded-Proto"] == "https";

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        Response.Cookies.Delete(AuthRoutes.Cookies.AccessToken, cookieOptions);
        Response.Cookies.Delete(AuthRoutes.Cookies.RefreshToken, cookieOptions);
    }

    private string? GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    #endregion
}
