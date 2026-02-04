using Audit.Abstractions.Entities;
using Audit.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Audit.API.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AuditDbContext _dbContext;

    public AuditLogRepository(AuditDbContext dbContext) => _dbContext = dbContext;

    public async Task<(List<AuditLog> Items, int Total)> GetLogsAsync(
        Guid? userId,
        string? action,
        string? entityType,
        string? serviceName,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.AuditLogs.AsQueryable();

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

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(List<AuditLog> Items, int Total)> GetEntityHistoryAsync(
        string entityType,
        string entityId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.AuditLogs
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public void Add(AuditLog entity) => _dbContext.AuditLogs.Add(entity);
}
