using Modules.Abstractions.Entities;

namespace Modules.Abstractions.Repositories;

public interface IModuleTypeRepository : IRepository<ModuleType, int>
{
    Task<(List<ModuleType> Items, int Total)> GetModuleTypesAsync(
        string? categoryId, bool? activeOnly, string? search,
        int? page, int? limit, CancellationToken ct = default);

    Task<int> GetModuleTypesCountAsync(
        string? categoryId, bool? activeOnly, string? search,
        CancellationToken ct = default);
}
