using Clients.Domain.Entities;

namespace Clients.Domain.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Client?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);
    Task<(List<Client> Items, int Total)> GetClientsAsync(
        Guid userId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken ct = default);
    void Add(Client entity);
    void Update(Client entity);
}
