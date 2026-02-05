using Microsoft.EntityFrameworkCore;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Repositories;

namespace Modules.API.Infrastructure.Persistence.Repositories;

public sealed class ComponentRepository : IComponentRepository
{
    private readonly ModulesDbContext _dbContext;

    public ComponentRepository(ModulesDbContext dbContext) => _dbContext = dbContext;

    public async Task<Component?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.Components.FindAsync([id], ct);

    public async Task<List<Component>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.Components.ToListAsync(ct);

    public void Add(Component entity) => _dbContext.Components.Add(entity);
    public void Update(Component entity) => _dbContext.Components.Update(entity);
    public void Remove(Component entity) => _dbContext.Components.Remove(entity);

    public async Task<List<Component>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
        => await _dbContext.Components.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

    public async Task<List<Component>> GetComponentsAsync(string? tag, bool? activeOnly, CancellationToken ct = default)
    {
        var query = _dbContext.Components.AsQueryable();

        if (!string.IsNullOrEmpty(tag))
            query = query.Where(c => c.Tags.Contains(tag));

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        return await query.ToListAsync(ct);
    }
}
