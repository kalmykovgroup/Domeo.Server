using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _dbContext;

    public RefreshTokenRepository(AuthDbContext dbContext) => _dbContext = dbContext;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, ct);

    public async Task<RefreshToken?> GetByTokenAndUserIdAsync(string token, Guid userId, CancellationToken ct = default)
        => await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token && x.UserId == userId, ct);

    public void Add(RefreshToken entity) => _dbContext.RefreshTokens.Add(entity);
}
