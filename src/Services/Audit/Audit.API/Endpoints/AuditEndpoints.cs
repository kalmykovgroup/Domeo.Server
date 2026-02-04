using Audit.API.Contracts;
using Audit.API.Persistence;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Audit.API.Endpoints;

public static class AuditEndpoints
{
    public static void MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/audit/logs").WithTags("Audit Logs");

        group.MapGet("/", GetAuditLogs).RequireAuthorization("Permission:audit:read");
        group.MapGet("/entity/{entityType}/{entityId}", GetEntityHistory).RequireAuthorization("Permission:audit:read");
    }

    private static async Task<IResult> GetAuditLogs(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] string? serviceName,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(l => l.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(l => l.ServiceName == serviceName);

        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 20;
        var total = await query.CountAsync(cancellationToken);

        var logs = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(l => new AuditLogResponse(
                l.Id,
                l.UserId,
                l.UserEmail,
                l.Action,
                l.EntityType,
                l.EntityId,
                l.ServiceName,
                l.OldValue,
                l.NewValue,
                l.IpAddress,
                l.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PaginatedResponse<AuditLogResponse>>.Ok(
            new PaginatedResponse<AuditLogResponse>(total, currentPage, currentPageSize, logs)));
    }

    private static async Task<IResult> GetEntityHistory(
        string entityType,
        string entityId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.CreatedAt);

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 20;
        var total = await query.CountAsync(cancellationToken);

        var logs = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(l => new AuditLogResponse(
                l.Id,
                l.UserId,
                l.UserEmail,
                l.Action,
                l.EntityType,
                l.EntityId,
                l.ServiceName,
                l.OldValue,
                l.NewValue,
                l.IpAddress,
                l.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PaginatedResponse<AuditLogResponse>>.Ok(
            new PaginatedResponse<AuditLogResponse>(total, currentPage, currentPageSize, logs)));
    }
}
