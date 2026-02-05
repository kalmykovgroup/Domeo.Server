using Modules.Abstractions.Entities;

namespace Modules.Abstractions.Repositories;

public interface IAssemblyRepository : IRepository<Assembly, Guid>
{
    Task<(List<Assembly> Items, int Total)> GetAssembliesAsync(
        string? categoryId, bool? activeOnly, string? search,
        int? page, int? limit, CancellationToken ct = default);

    Task<int> GetAssembliesCountAsync(
        string? categoryId, bool? activeOnly, string? search,
        CancellationToken ct = default);
}
