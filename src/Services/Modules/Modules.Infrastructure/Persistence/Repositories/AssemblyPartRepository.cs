using Microsoft.EntityFrameworkCore;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.Infrastructure.Persistence.Repositories;

public sealed class AssemblyPartRepository : IAssemblyPartRepository
{
    private readonly ModulesDbContext _dbContext;

    public AssemblyPartRepository(ModulesDbContext dbContext) => _dbContext = dbContext;

    public async Task<AssemblyPart?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.AssemblyParts.FindAsync([id], ct);

    public async Task<List<AssemblyPart>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.AssemblyParts.ToListAsync(ct);

    public void Add(AssemblyPart entity) => _dbContext.AssemblyParts.Add(entity);
    public void Update(AssemblyPart entity) => _dbContext.AssemblyParts.Update(entity);
    public void Remove(AssemblyPart entity) => _dbContext.AssemblyParts.Remove(entity);

    public async Task<List<AssemblyPart>> GetByAssemblyIdAsync(Guid assemblyId, CancellationToken ct = default)
    {
        return await _dbContext.AssemblyParts
            .Where(p => p.AssemblyId == assemblyId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);
    }
}
