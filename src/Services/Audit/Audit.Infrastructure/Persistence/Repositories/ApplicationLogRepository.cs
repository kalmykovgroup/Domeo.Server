using Audit.Domain.Entities;
using Audit.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Repositories;

public sealed class ApplicationLogRepository : IApplicationLogRepository
{
    private readonly AuditDbContext _dbContext;

    public ApplicationLogRepository(AuditDbContext dbContext) => _dbContext = dbContext;

    public async Task<ApplicationLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.ApplicationLogs.FindAsync([id], ct);

    public async Task<(List<ApplicationLog> Items, int Total)> GetLogsAsync(
        string? level,
        string? serviceName,
        Guid? userId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.ApplicationLogs.AsQueryable();

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

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public void Add(ApplicationLog entity) => _dbContext.ApplicationLogs.Add(entity);
}
