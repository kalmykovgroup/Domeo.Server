using Audit.Domain.Entities;

namespace Audit.Domain.Repositories;

public interface ILoginSessionRepository
{
    Task<LoginSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(List<LoginSession> Items, int Total)> GetSessionsAsync(
        Guid? userId,
        bool? activeOnly,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<(List<LoginSession> Items, int Total)> GetUserSessionsAsync(
        Guid userId,
        bool? activeOnly,
        int page,
        int pageSize,
        CancellationToken ct = default);

    void Add(LoginSession entity);
}
