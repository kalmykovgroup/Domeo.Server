using Auth.Abstractions.Entities;

namespace Auth.Abstractions.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<RefreshToken?> GetByTokenAndUserIdAsync(string token, Guid userId, CancellationToken ct = default);
    void Add(RefreshToken entity);
}
