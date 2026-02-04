using Microsoft.EntityFrameworkCore;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Repositories;

namespace Modules.API.Infrastructure.Persistence.Repositories;

public sealed class ModuleTypeRepository : IModuleTypeRepository
{
    private readonly ModulesDbContext _dbContext;

    public ModuleTypeRepository(ModulesDbContext dbContext) => _dbContext = dbContext;

    public async Task<ModuleType?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbContext.ModuleTypes.FindAsync([id], ct);

    public async Task<List<ModuleType>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.ModuleTypes.ToListAsync(ct);

    public void Add(ModuleType entity) => _dbContext.ModuleTypes.Add(entity);
    public void Update(ModuleType entity) => _dbContext.ModuleTypes.Update(entity);
    public void Remove(ModuleType entity) => _dbContext.ModuleTypes.Remove(entity);

    public async Task<(List<ModuleType> Items, int Total)> GetModuleTypesAsync(
        string? categoryId, bool? activeOnly, string? search,
        int? page, int? limit, CancellationToken ct = default)
    {
        var query = BuildQuery(categoryId, activeOnly, search);

        if (page.HasValue && limit.HasValue)
        {
            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page.Value - 1) * limit.Value)
                .Take(limit.Value)
                .ToListAsync(ct);

            return (items, total);
        }

        var allItems = await query.ToListAsync(ct);
        return (allItems, allItems.Count);
    }

    public async Task<int> GetModuleTypesCountAsync(
        string? categoryId, bool? activeOnly, string? search,
        CancellationToken ct = default)
    {
        var query = BuildQuery(categoryId, activeOnly, search);
        return await query.CountAsync(ct);
    }

    private IQueryable<ModuleType> BuildQuery(string? categoryId, bool? activeOnly, string? search)
    {
        var query = _dbContext.ModuleTypes.AsQueryable();

        if (!string.IsNullOrEmpty(categoryId))
            query = query.Where(m => m.CategoryId == categoryId);

        if (activeOnly == true)
            query = query.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(searchLower) ||
                                     m.Type.ToLower().Contains(searchLower));
        }

        return query;
    }
}
