using Microsoft.EntityFrameworkCore;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.Infrastructure.Persistence.Repositories;

public sealed class StorageConnectionRepository : IStorageConnectionRepository
{
    private readonly ModulesDbContext _dbContext;

    public StorageConnectionRepository(ModulesDbContext dbContext) => _dbContext = dbContext;

    public async Task<StorageConnection?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.StorageConnections.FindAsync([id], ct);

    public async Task<List<StorageConnection>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.StorageConnections.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);

    public void Add(StorageConnection entity) => _dbContext.StorageConnections.Add(entity);
    public void Update(StorageConnection entity) => _dbContext.StorageConnections.Update(entity);
    public void Remove(StorageConnection entity) => _dbContext.StorageConnections.Remove(entity);

    public async Task<StorageConnection?> GetActiveAsync(CancellationToken ct = default)
        => await _dbContext.StorageConnections.FirstOrDefaultAsync(s => s.IsActive, ct);
}
