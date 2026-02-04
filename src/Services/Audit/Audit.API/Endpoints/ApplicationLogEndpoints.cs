using Audit.API.Contracts;
using Audit.API.Persistence;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Audit.API.Endpoints;

public static class ApplicationLogEndpoints
{
    public static void MapApplicationLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/audit/application-logs").WithTags("Application Logs");

        group.MapGet("/", GetApplicationLogs).RequireAuthorization("Permission:audit:read");
        group.MapGet("/{id:guid}", GetApplicationLog).RequireAuthorization("Permission:audit:read");
        group.MapGet("/stats", GetLogStats).RequireAuthorization("Permission:audit:read");
    }

    private static async Task<IResult> GetApplicationLogs(
        [FromQuery] string? level,
        [FromQuery] string? serviceName,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ApplicationLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(l => l.Level == level);

        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(l => l.ServiceName == serviceName);

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);

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
            .Select(l => new ApplicationLogResponse(
                l.Id,
                l.ServiceName,
                l.Level,
                l.Message,
                l.Exception,
                l.ExceptionType,
                l.Properties,
                l.RequestPath,
                l.UserId,
                l.CorrelationId,
                l.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PaginatedResponse<ApplicationLogResponse>>.Ok(
            new PaginatedResponse<ApplicationLogResponse>(total, currentPage, currentPageSize, logs)));
    }

    private static async Task<IResult> GetApplicationLog(
        Guid id,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var log = await dbContext.ApplicationLogs
            .Where(l => l.Id == id)
            .Select(l => new ApplicationLogResponse(
                l.Id,
                l.ServiceName,
                l.Level,
                l.Message,
                l.Exception,
                l.ExceptionType,
                l.Properties,
                l.RequestPath,
                l.UserId,
                l.CorrelationId,
                l.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null)
            return Results.NotFound(ApiResponse<object>.Fail("Application log not found"));

        return Results.Ok(ApiResponse<ApplicationLogResponse>.Ok(log));
    }

    private static async Task<IResult> GetLogStats(
        [FromQuery] string? groupBy,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        AuditDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ApplicationLogs.AsQueryable();

        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);

        var groupByFields = (groupBy ?? "service,level").Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (groupByFields.Contains("service") && groupByFields.Contains("level"))
        {
            var stats = await query
                .GroupBy(l => new { l.ServiceName, l.Level })
                .Select(g => new
                {
                    ServiceName = g.Key.ServiceName,
                    Level = g.Key.Level,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<object>.Ok(stats));
        }

        if (groupByFields.Contains("service"))
        {
            var stats = await query
                .GroupBy(l => l.ServiceName)
                .Select(g => new
                {
                    ServiceName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<object>.Ok(stats));
        }

        if (groupByFields.Contains("level"))
        {
            var stats = await query
                .GroupBy(l => l.Level)
                .Select(g => new
                {
                    Level = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<object>.Ok(stats));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        return Results.Ok(ApiResponse<object>.Ok(new { Total = totalCount }));
    }
}
