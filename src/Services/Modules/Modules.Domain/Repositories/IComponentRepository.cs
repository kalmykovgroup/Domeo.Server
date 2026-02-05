using Modules.Domain.Entities;

namespace Modules.Domain.Repositories;

public interface IComponentRepository : IRepository<Component, Guid>
{
    Task<List<Component>> GetComponentsAsync(string? tag, bool? activeOnly, CancellationToken ct = default);
    Task<List<Component>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);
}
