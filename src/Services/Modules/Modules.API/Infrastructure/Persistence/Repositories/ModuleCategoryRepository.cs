using Microsoft.EntityFrameworkCore;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Repositories;

namespace Modules.API.Infrastructure.Persistence.Repositories;

public sealed class ModuleCategoryRepository : IModuleCategoryRepository
{
    private readonly ModulesDbContext _dbContext;

    public ModuleCategoryRepository(ModulesDbContext dbContext) => _dbContext = dbContext;

    public async Task<ModuleCategory?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _dbContext.ModuleCategories.FindAsync([id], ct);

    public async Task<List<ModuleCategory>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.ModuleCategories.ToListAsync(ct);

    public void Add(ModuleCategory entity) => _dbContext.ModuleCategories.Add(entity);
    public void Update(ModuleCategory entity) => _dbContext.ModuleCategories.Update(entity);
    public void Remove(ModuleCategory entity) => _dbContext.ModuleCategories.Remove(entity);

    public async Task<List<ModuleCategory>> GetCategoriesAsync(bool? activeOnly, CancellationToken ct = default)
    {
        var query = _dbContext.ModuleCategories.AsQueryable();

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        return await query.OrderBy(c => c.OrderIndex).ToListAsync(ct);
    }
}
