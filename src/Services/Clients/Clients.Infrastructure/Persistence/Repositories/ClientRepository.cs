using Clients.Domain.Entities;
using Clients.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository : IClientRepository
{
    private readonly ClientsDbContext _dbContext;

    public ClientRepository(ClientsDbContext dbContext) => _dbContext = dbContext;

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.Clients.FindAsync([id], ct);

    public async Task<Client?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.Clients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<(List<Client> Items, int Total)> GetClientsAsync(
        Guid userId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.Clients
            .Where(c => c.UserId == userId && c.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                (c.Phone != null && c.Phone.Contains(search)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)));
        }

        query = sortBy?.ToLower() switch
        {
            "name" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "createdat" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public void Add(Client entity) => _dbContext.Clients.Add(entity);
    public void Update(Client entity) => _dbContext.Clients.Update(entity);
}
