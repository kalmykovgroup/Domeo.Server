using System.Collections.Concurrent;

namespace MockAuthCenter.API.Services;

public class RefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenEntry> _tokens = new();
    private readonly int _lifetimeDays;

    public RefreshTokenStore(IConfiguration configuration)
    {
        _lifetimeDays = configuration.GetValue<int>("AuthCenter:RefreshTokenLifetimeDays", 7);
    }

    public string CreateToken(string userId, string clientId)
    {
        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var entry = new RefreshTokenEntry
        {
            Token = token,
            UserId = userId,
            ClientId = clientId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_lifetimeDays)
        };

        _tokens[token] = entry;
        CleanupExpired();

        return token;
    }

    public RefreshTokenEntry? GetToken(string token)
    {
        if (_tokens.TryGetValue(token, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return entry;
            }
            _tokens.TryRemove(token, out _);
        }
        return null;
    }

    public string RotateToken(string oldToken, string userId, string clientId)
    {
        _tokens.TryRemove(oldToken, out _);
        return CreateToken(userId, clientId);
    }

    public void RevokeToken(string token)
    {
        _tokens.TryRemove(token, out _);
    }

    public void RevokeAllUserTokens(string userId)
    {
        var userTokens = _tokens
            .Where(kvp => kvp.Value.UserId == userId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in userTokens)
        {
            _tokens.TryRemove(token, out _);
        }
    }

    private void CleanupExpired()
    {
        var expiredTokens = _tokens
            .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in expiredTokens)
        {
            _tokens.TryRemove(token, out _);
        }
    }
}

public class RefreshTokenEntry
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
