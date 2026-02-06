using Modules.Domain.Entities;

namespace Modules.Domain.Repositories;

public interface IStorageConnectionRepository : IRepository<StorageConnection, Guid>
{
    Task<StorageConnection?> GetActiveAsync(CancellationToken ct = default);
}
