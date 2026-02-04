using Microsoft.EntityFrameworkCore;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Repositories;

namespace Modules.API.Infrastructure.Persistence.Repositories;

public sealed class HardwareRepository : IHardwareRepository
{
    private readonly ModulesDbContext _dbContext;

    public HardwareRepository(ModulesDbContext dbContext) => _dbContext = dbContext;

    public async Task<Hardware?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbContext.Hardware.FindAsync([id], ct);

    public async Task<List<Hardware>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.Hardware.ToListAsync(ct);

    public void Add(Hardware entity) => _dbContext.Hardware.Add(entity);
    public void Update(Hardware entity) => _dbContext.Hardware.Update(entity);
    public void Remove(Hardware entity) => _dbContext.Hardware.Remove(entity);

    public async Task<List<Hardware>> GetHardwareAsync(string? type, bool? activeOnly, CancellationToken ct = default)
    {
        var query = _dbContext.Hardware.AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(h => h.Type == type);

        if (activeOnly == true)
            query = query.Where(h => h.IsActive);

        return await query.ToListAsync(ct);
    }
}
