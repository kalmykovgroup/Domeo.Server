using Modules.Abstractions.Entities;

namespace Modules.Abstractions.Repositories;

public interface IComponentRepository : IRepository<Component, Guid>
{
    Task<List<Component>> GetComponentsAsync(string? tag, bool? activeOnly, CancellationToken ct = default);
}
