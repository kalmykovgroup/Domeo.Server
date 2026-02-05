using Modules.Domain.Entities;

namespace Modules.Domain.Repositories;

public interface IModuleCategoryRepository : IRepository<ModuleCategory, string>
{
    Task<List<ModuleCategory>> GetCategoriesAsync(bool? activeOnly, CancellationToken ct = default);
}
