using Audit.API.Contracts;
using Audit.API.Entities;
using Audit.API.Persistence;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Audit.API.Endpoints;

public static class LoginSessionEndpoints
{
    public static void MapLoginSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/audit/login-sessions").WithTags("Login Sessions");

        // Internal API - called by Auth.API
        group.MapPost("/", CreateLoginSession).RequireAuthorization("InternalApi");
        group.MapPut("/{id:guid}/logout", LogoutSession).RequireAuthorization("InternalApi");

        // Read endpoints - for admin viewing
        group.MapGet("/", GetLoginSessions).RequireAuthorization("Permission:audit:read");
        group.MapGet("/{id:guid}", GetLoginSession).RequireAuthorization("Permission:audit:read");
        group.MapGet("/user/{userId:guid}", GetUserSessions).RequireAuthorization("Permission:audit:read");
    }

    private static async Task<IResult> CreateLoginSession(
        [FromBody] CreateLoginSessionRequest request,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var session = LoginSession.Create(
            Guid.NewGuid(),
            request.UserId,
            request.UserEmail,
            request.UserName,
            request.UserRole,
            request.IpAddress,
            request.UserAgent);

        dbContext.LoginSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<LoginSessionDto>.Ok(ToDto(session), "Login session created"));
    }

    private static async Task<IResult> LogoutSession(
        Guid id,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.LoginSessions.FindAsync([id], cancellationToken);
        if (session is null)
            return Results.Ok(ApiResponse.Fail("Session not found"));

        if (!session.IsActive)
            return Results.Ok(ApiResponse.Fail("Session already logged out"));

        session.Logout();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Logout recorded"));
    }

    private static async Task<IResult> GetLoginSessions(
        [FromQuery] Guid? userId,
        [FromQuery] bool? activeOnly,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.LoginSessions.AsQueryable();

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        if (activeOnly == true)
            query = query.Where(s => s.LoggedOutAt == null);

        if (from.HasValue)
            query = query.Where(s => s.LoggedInAt >= from.Value);

        if (to.HasValue)
            query = query.Where(s => s.LoggedInAt <= to.Value);

        query = query.OrderByDescending(s => s.LoggedInAt);

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 20;
        var total = await query.CountAsync(cancellationToken);

        var sessions = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(s => ToDto(s))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PaginatedResponse<LoginSessionDto>>.Ok(
            new PaginatedResponse<LoginSessionDto>(total, currentPage, currentPageSize, sessions)));
    }

    private static async Task<IResult> GetLoginSession(
        Guid id,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.LoginSessions.FindAsync([id], cancellationToken);
        if (session is null)
            return Results.Ok(ApiResponse<LoginSessionDto>.Fail("Session not found"));

        return Results.Ok(ApiResponse<LoginSessionDto>.Ok(ToDto(session)));
    }

    private static async Task<IResult> GetUserSessions(
        Guid userId,
        [FromQuery] bool? activeOnly,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.LoginSessions.Where(s => s.UserId == userId);

        if (activeOnly == true)
            query = query.Where(s => s.LoggedOutAt == null);

        query = query.OrderByDescending(s => s.LoggedInAt);

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 20;
        var total = await query.CountAsync(cancellationToken);

        var sessions = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(s => ToDto(s))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PaginatedResponse<LoginSessionDto>>.Ok(
            new PaginatedResponse<LoginSessionDto>(total, currentPage, currentPageSize, sessions)));
    }

    private static LoginSessionDto ToDto(LoginSession session) => new(
        session.Id,
        session.UserId,
        session.UserEmail,
        session.UserName,
        session.UserRole,
        session.IpAddress,
        session.UserAgent,
        session.LoggedInAt,
        session.LoggedOutAt,
        session.IsActive);
}
