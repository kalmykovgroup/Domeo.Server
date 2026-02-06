using Microsoft.EntityFrameworkCore;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.Infrastructure.Persistence.Repositories;

public sealed class AssemblyRepository : IAssemblyRepository
{
    private readonly ModulesDbContext _dbContext;

    public AssemblyRepository(ModulesDbContext dbContext) => _dbContext = dbContext;

    public async Task<Assembly?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.Assemblies.FindAsync([id], ct);

    public async Task<List<Assembly>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.Assemblies.ToListAsync(ct);

    public void Add(Assembly entity) => _dbContext.Assemblies.Add(entity);
    public void Update(Assembly entity) => _dbContext.Assemblies.Update(entity);
    public void Remove(Assembly entity) => _dbContext.Assemblies.Remove(entity);

    public async Task<(List<Assembly> Items, int Total)> GetAssembliesAsync(
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

    private IQueryable<Assembly> BuildQuery(string? categoryId, bool? activeOnly, string? search)
    {
        var query = _dbContext.Assemblies.AsQueryable();

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
