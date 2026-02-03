using System.IdentityModel.Tokens.Jwt;
using Auth.API.Contracts;
using Auth.API.Entities;
using Auth.API.Persistence;
using Auth.API.Services;
using Domeo.Shared.Auth;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
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

        // OAuth callback (primary auth method)
        group.MapPost("/callback", Callback);

        // Legacy direct auth (for mobile/testing)
        group.MapPost("/register", Register);
        group.MapPost("/login", Login);

        // Token management
        group.MapPost("/refresh", RefreshTokenEndpoint);
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
        group.MapPut("/me/password", ChangePassword).RequireAuthorization();
        group.MapGet("/token", GetToken);
    }

    private static bool IsBrowserClient(HttpRequest request)
    {
        var userAgent = request.Headers.UserAgent.ToString();
        var clientType = request.Headers["X-Client-Type"].ToString().ToLower();

        // Если клиент явно указал тип "mobile", это не браузер
        if (clientType == "mobile")
            return false;

        // Проверяем User-Agent на признаки браузера
        return userAgent.Contains("Mozilla") || userAgent.Contains("Chrome") || userAgent.Contains("Safari");
    }

    private static void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken, DateTime accessTokenExpires, DateTime refreshTokenExpires)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = accessTokenExpires
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = refreshTokenExpires
        };

        response.Cookies.Append(AccessTokenCookieName, accessToken, accessCookieOptions);
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshCookieOptions);
    }

    private static void ClearAuthCookies(HttpResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        response.Cookies.Delete(AccessTokenCookieName, cookieOptions);
        response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);
    }

    /// <summary>
    /// OAuth callback - exchange authorization code for tokens
    /// </summary>
    private static async Task<IResult> Callback(
        HttpContext httpContext,
        [FromBody] CallbackRequest request,
        IAuthCenterClient authCenterClient,
        IHttpClientFactory httpClientFactory,
        IPasswordHasher passwordHasher,
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Exchange code for tokens at Auth Center
        var tokenResponse = await authCenterClient.ExchangeCodeAsync(request.Code, request.RedirectUri, cancellationToken);
        if (tokenResponse is null)
        {
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Invalid authorization code"));
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
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Invalid token claims"));
        }

        // Find or create user in Users.API
        var client = httpClientFactory.CreateClient("UsersApi");
        var checkResponse = await client.GetAsync($"/users/by-email/{Uri.EscapeDataString(email)}", cancellationToken);

        UserDto? user;
        if (checkResponse.IsSuccessStatusCode)
        {
            var existingUser = await checkResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(cancellationToken);
            user = existingUser?.Data;
        }
        else
        {
            // Create user - split name into first/last
            var nameParts = (name ?? email.Split('@')[0]).Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var createUserRequest = new
            {
                Email = email,
                PasswordHash = passwordHasher.Hash(Guid.NewGuid().ToString()), // Random password for SSO users
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                ExternalId = userId
            };

            var createResponse = await client.PostAsJsonAsync("/users", createUserRequest, cancellationToken);
            if (!createResponse.IsSuccessStatusCode)
            {
                return Results.Ok(ApiResponse<AuthResultDto>.Fail("Failed to create user"));
            }

            var userResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(cancellationToken);
            user = userResponse?.Data;
        }

        if (user is null)
        {
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Failed to get user"));
        }

        // Store Auth Center refresh token in our DB for later use
        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenResponse.RefreshToken,
            TimeSpan.FromDays(7));

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        // Set cookies for browser clients
        if (IsBrowserClient(httpContext.Request))
        {
            SetAuthCookies(
                httpContext.Response,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                accessTokenExpiration,
                DateTime.UtcNow.AddDays(7));
        }

        var tokenDto = new TokenDto(tokenResponse.AccessToken, tokenResponse.RefreshToken, accessTokenExpiration);

        return Results.Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(user, tokenDto)));
    }

    private static async Task<IResult> Register(
        HttpContext httpContext,
        [FromBody] RegisterRequest request,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtGenerator,
        AuthDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        // Check if user already exists
        var client = httpClientFactory.CreateClient("UsersApi");
        var checkResponse = await client.GetAsync($"/users/by-email/{Uri.EscapeDataString(request.Email)}", cancellationToken);

        if (checkResponse.IsSuccessStatusCode)
        {
            var existingUser = await checkResponse.Content.ReadFromJsonAsync<ApiResponse<UserWithPasswordDto>>(cancellationToken);
            if (existingUser?.Success == true && existingUser.Data is not null)
                return Results.Ok(ApiResponse<AuthResultDto>.Fail("User with this email already exists"));
        }

        // Create user in Users.API
        var passwordHash = passwordHasher.Hash(request.Password);
        var createUserRequest = new
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var createResponse = await client.PostAsJsonAsync("/users", createUserRequest, cancellationToken);
        if (!createResponse.IsSuccessStatusCode)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Failed to create user"));

        var userResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(cancellationToken);
        if (userResponse?.Data is null)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Failed to create user"));

        var user = userResponse.Data;

        // Generate tokens
        var accessToken = jwtGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Role);

        var refreshTokenValue = jwtGenerator.GenerateRefreshToken();
        var refreshTokenLifetime = jwtGenerator.GetRefreshTokenLifetime();

        // Store refresh token
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            refreshTokenLifetime);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = jwtGenerator.GetAccessTokenExpiration();

        // Set cookies for browser clients
        if (IsBrowserClient(httpContext.Request))
        {
            SetAuthCookies(
                httpContext.Response,
                accessToken,
                refreshTokenValue,
                accessTokenExpiration,
                DateTime.UtcNow.Add(refreshTokenLifetime));
        }

        // Build response (token also returned for WebSocket/mobile)
        var tokenDto = new TokenDto(accessToken, refreshTokenValue, accessTokenExpiration);

        return Results.Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(user, tokenDto)));
    }

    private static async Task<IResult> Login(
        HttpContext httpContext,
        [FromBody] LoginRequest request,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtGenerator,
        AuthDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        // Get user from Users.API
        var client = httpClientFactory.CreateClient("UsersApi");
        var response = await client.GetAsync($"/users/by-email/{Uri.EscapeDataString(request.Email)}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Invalid credentials"));

        var userResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserWithPasswordDto>>(cancellationToken);
        if (userResponse?.Data is null)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Invalid credentials"));

        var user = userResponse.Data;

        // Verify password
        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Invalid credentials"));

        // Check if user is active
        if (!user.IsActive)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Account is deactivated"));

        // Generate tokens
        var accessToken = jwtGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Role);

        var refreshTokenValue = jwtGenerator.GenerateRefreshToken();
        var refreshTokenLifetime = jwtGenerator.GetRefreshTokenLifetime();

        // Store refresh token
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            refreshTokenLifetime);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = jwtGenerator.GetAccessTokenExpiration();

        // Set cookies for browser clients
        if (IsBrowserClient(httpContext.Request))
        {
            SetAuthCookies(
                httpContext.Response,
                accessToken,
                refreshTokenValue,
                accessTokenExpiration,
                DateTime.UtcNow.Add(refreshTokenLifetime));
        }

        // Build response (token also returned for WebSocket/mobile)
        var tokenDto = new TokenDto(accessToken, refreshTokenValue, accessTokenExpiration);
        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.CreatedAt);

        return Results.Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(userDto, tokenDto)));
    }

    private static async Task<IResult> RefreshTokenEndpoint(
        HttpContext httpContext,
        [FromBody] RefreshTokenRequest? request,
        IJwtTokenGenerator jwtGenerator,
        AuthDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        // Try to get refresh token from request body or cookie
        var refreshTokenValue = request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshTokenValue);
        }

        if (string.IsNullOrEmpty(refreshTokenValue))
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Refresh token is required"));

        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshTokenValue, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Invalid or expired refresh token"));

        // Get user info
        var client = httpClientFactory.CreateClient("UsersApi");
        var response = await client.GetAsync($"/users/{refreshToken.UserId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("User not found"));

        var userResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(cancellationToken);
        if (userResponse?.Data is null)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("User not found"));

        var user = userResponse.Data;

        if (!user.IsActive)
            return Results.Ok(ApiResponse<AuthResultDto>.Fail("Account is deactivated"));

        // Revoke old token
        refreshToken.Revoke();

        // Create new tokens
        var newRefreshTokenValue = jwtGenerator.GenerateRefreshToken();
        var refreshTokenLifetime = jwtGenerator.GetRefreshTokenLifetime();
        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newRefreshTokenValue,
            refreshTokenLifetime);

        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = jwtGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Role);

        var accessTokenExpiration = jwtGenerator.GetAccessTokenExpiration();

        // Set cookies for browser clients
        if (IsBrowserClient(httpContext.Request))
        {
            SetAuthCookies(
                httpContext.Response,
                accessToken,
                newRefreshTokenValue,
                accessTokenExpiration,
                DateTime.UtcNow.Add(refreshTokenLifetime));
        }

        var tokenDto = new TokenDto(accessToken, newRefreshTokenValue, accessTokenExpiration);

        return Results.Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(user, tokenDto)));
    }

    private static async Task<IResult> Logout(
        HttpContext httpContext,
        [FromBody] LogoutRequest? request,
        ICurrentUserAccessor currentUserAccessor,
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse.Fail("Unauthorized"));

        // Try to get refresh token from request body or cookie
        var refreshTokenValue = request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshTokenValue);
        }

        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            var refreshToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshTokenValue && x.UserId == userId, cancellationToken);

            if (refreshToken is not null)
            {
                refreshToken.Revoke();
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        // Clear cookies
        ClearAuthCookies(httpContext.Response);

        return Results.Ok(ApiResponse.Ok("Logged out successfully"));
    }

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

    private static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        ICurrentUserAccessor currentUserAccessor,
        IPasswordHasher passwordHasher,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse.Fail("Unauthorized"));

        // Get current user with password
        var client = httpClientFactory.CreateClient("UsersApi");
        var response = await client.GetAsync($"/users/{userId}/with-password", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Results.Ok(ApiResponse.Fail("User not found"));

        var userResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserWithPasswordDto>>(cancellationToken);
        if (userResponse?.Data is null)
            return Results.Ok(ApiResponse.Fail("User not found"));

        // Verify current password
        if (!passwordHasher.Verify(request.CurrentPassword, userResponse.Data.PasswordHash))
            return Results.Ok(ApiResponse.Fail("Current password is incorrect"));

        // Update password
        var newPasswordHash = passwordHasher.Hash(request.NewPassword);
        var updateResponse = await client.PutAsJsonAsync(
            $"/users/{userId}/password",
            new { PasswordHash = newPasswordHash },
            cancellationToken);

        if (!updateResponse.IsSuccessStatusCode)
            return Results.Ok(ApiResponse.Fail("Failed to update password"));

        return Results.Ok(ApiResponse.Ok("Password changed successfully"));
    }

    /// <summary>
    /// Get JWT token from HttpOnly cookie for WebSocket authentication
    /// Uses In-Band Authentication pattern
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

internal sealed record UserWithPasswordDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    string PasswordHash,
    DateTime CreatedAt)
{
    [System.Text.Json.Serialization.JsonConstructor]
    public UserWithPasswordDto() : this(Guid.Empty, "", "", "", "", false, "", DateTime.MinValue) { }
}
