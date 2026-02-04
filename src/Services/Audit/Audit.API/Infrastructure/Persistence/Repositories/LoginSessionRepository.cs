using Audit.Abstractions.Entities;
using Audit.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Audit.API.Infrastructure.Persistence.Repositories;

public sealed class LoginSessionRepository : ILoginSessionRepository
{
    private readonly AuditDbContext _dbContext;

    public LoginSessionRepository(AuditDbContext dbContext) => _dbContext = dbContext;

    public async Task<LoginSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.LoginSessions.FindAsync([id], ct);

    public async Task<(List<LoginSession> Items, int Total)> GetSessionsAsync(
        Guid? userId,
        bool? activeOnly,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.LoginSessions.AsQueryable();

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        if (activeOnly == true)
            query = query.Where(s => s.LoggedOutAt == null);

        if (from.HasValue)
            query = query.Where(s => s.LoggedInAt >= from.Value);

        if (to.HasValue)
            query = query.Where(s => s.LoggedInAt <= to.Value);

        query = query.OrderByDescending(s => s.LoggedInAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(List<LoginSession> Items, int Total)> GetUserSessionsAsync(
        Guid userId,
        bool? activeOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.LoginSessions.Where(s => s.UserId == userId);

        if (activeOnly == true)
            query = query.Where(s => s.LoggedOutAt == null);

        query = query.OrderByDescending(s => s.LoggedInAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public void Add(LoginSession entity) => _dbContext.LoginSessions.Add(entity);
}
