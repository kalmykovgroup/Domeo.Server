using Audit.Application.Queries.ApplicationLogs;
using Audit.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audit.API.Application.ApplicationLogs.Queries;

public sealed class GetApplicationLogStatsQueryHandler
    : IRequestHandler<GetApplicationLogStatsQuery, object>
{
    private readonly AuditDbContext _dbContext;

    public GetApplicationLogStatsQueryHandler(AuditDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<object> Handle(
        GetApplicationLogStatsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ApplicationLogs.AsQueryable();

        if (request.From.HasValue)
            query = query.Where(l => l.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(l => l.CreatedAt <= request.To.Value);

        var groupByFields = (request.GroupBy ?? "service,level")
            .Split(',', StringSplitOptions.RemoveEmptyEntries);

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

            return stats;
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

            return stats;
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

            return stats;
        }

        var totalCount = await query.CountAsync(cancellationToken);
        return new { Total = totalCount };
    }
}
