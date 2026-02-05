using Audit.Domain.Entities;

namespace Audit.Domain.Repositories;

public interface IApplicationLogRepository
{
    Task<ApplicationLog?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(List<ApplicationLog> Items, int Total)> GetLogsAsync(
        string? level,
        string? serviceName,
        Guid? userId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default);

    void Add(ApplicationLog entity);
}
