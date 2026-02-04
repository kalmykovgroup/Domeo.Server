using Modules.Abstractions.Entities;

namespace Modules.Abstractions.Repositories;

public interface IModuleCategoryRepository : IRepository<ModuleCategory, string>
{
    Task<List<ModuleCategory>> GetCategoriesAsync(bool? activeOnly, CancellationToken ct = default);
}
