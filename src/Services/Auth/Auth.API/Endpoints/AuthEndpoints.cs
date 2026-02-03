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
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/refresh", RefreshTokenEndpoint);
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
        group.MapPut("/me/password", ChangePassword).RequireAuthorization();
    }

    private static async Task<IResult> Register(
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

        // Store refresh token
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            jwtGenerator.GetRefreshTokenLifetime());

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Build response
        var tokenDto = new TokenDto(accessToken, refreshTokenValue, jwtGenerator.GetAccessTokenExpiration());

        return Results.Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(user, tokenDto)));
    }

    private static async Task<IResult> Login(
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

        // Store refresh token
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            jwtGenerator.GetRefreshTokenLifetime());

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Build response
        var tokenDto = new TokenDto(accessToken, refreshTokenValue, jwtGenerator.GetAccessTokenExpiration());
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
        [FromBody] RefreshTokenRequest request,
        IJwtTokenGenerator jwtGenerator,
        AuthDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken);

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
        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newRefreshTokenValue,
            jwtGenerator.GetRefreshTokenLifetime());

        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = jwtGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Role);

        var tokenDto = new TokenDto(accessToken, newRefreshTokenValue, jwtGenerator.GetAccessTokenExpiration());

        return Results.Ok(ApiResponse<AuthResultDto>.Ok(new AuthResultDto(user, tokenDto)));
    }

    private static async Task<IResult> Logout(
        [FromBody] LogoutRequest request,
        ICurrentUserAccessor currentUserAccessor,
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse.Fail("Unauthorized"));

        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken && x.UserId == userId, cancellationToken);

        if (refreshToken is not null)
        {
            refreshToken.Revoke();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

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
