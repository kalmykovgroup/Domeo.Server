using Audit.Domain.Entities;

namespace Audit.Domain.Repositories;

public interface IAuditLogRepository
{
    Task<(List<AuditLog> Items, int Total)> GetLogsAsync(
        Guid? userId,
        string? action,
        string? entityType,
        string? serviceName,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<(List<AuditLog> Items, int Total)> GetEntityHistoryAsync(
        string entityType,
        string entityId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    void Add(AuditLog entity);
}
