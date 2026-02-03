using Domeo.Shared.Auth;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.API.Contracts;
using Users.API.Entities;
using Users.API.Persistence;

namespace Users.API.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        // Admin endpoints
        group.MapGet("/", GetUsers).RequireAuthorization("Permission:users:read");
        group.MapGet("/{id:guid}", GetUser).RequireAuthorization("Permission:users:read");

        // Internal endpoints - service-to-service communication (requires internal API key)
        group.MapGet("/by-email/{email}", GetUserByEmail).RequireAuthorization("InternalApi");
        group.MapGet("/{id:guid}/with-password", GetUserWithPassword).RequireAuthorization("InternalApi");
        group.MapPost("/", CreateUser).RequireAuthorization("InternalApi");
        group.MapPut("/{id:guid}/password", UpdatePassword).RequireAuthorization("InternalApi");

        // Self-service endpoints - any authenticated user
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
        group.MapPut("/me", UpdateCurrentUser).RequireAuthorization();
    }

    private static async Task<IResult> GetUsers(
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.IsActive,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<UserDto>>.Ok(users));
    }

    private static async Task<IResult> GetUser(
        Guid id,
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null)
            return Results.Ok(ApiResponse<UserDto>.Fail("User not found"));

        return Results.Ok(ApiResponse<UserDto>.Ok(new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.CreatedAt)));
    }

    private static async Task<IResult> GetUserByEmail(
        string email,
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var decodedEmail = Uri.UnescapeDataString(email);
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == decodedEmail, cancellationToken);

        if (user is null)
            return Results.Ok(ApiResponse<UserWithPasswordDto>.Fail("User not found"));

        return Results.Ok(ApiResponse<UserWithPasswordDto>.Ok(new UserWithPasswordDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.PasswordHash,
            user.CreatedAt)));
    }

    private static async Task<IResult> GetUserWithPassword(
        Guid id,
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null)
            return Results.Ok(ApiResponse<UserWithPasswordDto>.Fail("User not found"));

        return Results.Ok(ApiResponse<UserWithPasswordDto>.Ok(new UserWithPasswordDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.PasswordHash,
            user.CreatedAt)));
    }

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser is not null)
            return Results.Ok(ApiResponse<UserDto>.Fail("User with this email already exists"));

        var user = User.Create(
            request.Email,
            request.PasswordHash,
            request.FirstName,
            request.LastName);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<UserDto>.Ok(new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.CreatedAt)));
    }

    private static async Task<IResult> UpdatePassword(
        Guid id,
        [FromBody] UpdatePasswordRequest request,
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null)
            return Results.Ok(ApiResponse.Fail("User not found"));

        user.UpdatePassword(request.PasswordHash);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Password updated successfully"));
    }

    private static async Task<IResult> GetCurrentUser(
        ICurrentUserAccessor currentUserAccessor,
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<UserDto>.Fail("Unauthorized"));

        var user = await dbContext.Users.FindAsync([userId.Value], cancellationToken);
        if (user is null)
            return Results.Ok(ApiResponse<UserDto>.Fail("User not found"));

        return Results.Ok(ApiResponse<UserDto>.Ok(new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.CreatedAt)));
    }

    private static async Task<IResult> UpdateCurrentUser(
        [FromBody] UpdateUserRequest request,
        ICurrentUserAccessor currentUserAccessor,
        UsersDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<UserDto>.Fail("Unauthorized"));

        var user = await dbContext.Users.FindAsync([userId.Value], cancellationToken);
        if (user is null)
            return Results.Ok(ApiResponse<UserDto>.Fail("User not found"));

        user.Update(request.FirstName, request.LastName);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<UserDto>.Ok(new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.CreatedAt)));
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
    DateTime CreatedAt);
