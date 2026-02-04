using System.Collections.Concurrent;

namespace MockAuthCenter.API.Services;

public class AuthCodeStore
{
    private readonly ConcurrentDictionary<string, AuthCodeEntry> _codes = new();
    private readonly int _lifetimeSeconds;

    public AuthCodeStore(IConfiguration configuration)
    {
        _lifetimeSeconds = configuration.GetValue<int>("AuthCenter:AuthCodeLifetimeSeconds", 60);
    }

    public string CreateCode(string userId, string clientId, string redirectUri, string? state)
    {
        var code = Guid.NewGuid().ToString("N");
        var entry = new AuthCodeEntry
        {
            Code = code,
            UserId = userId,
            ClientId = clientId,
            RedirectUri = redirectUri,
            State = state,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(_lifetimeSeconds)
        };

        _codes[code] = entry;
        CleanupExpired();

        return code;
    }

    public AuthCodeEntry? ConsumeCode(string code)
    {
        if (_codes.TryRemove(code, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return entry;
            }
        }
        return null;
    }

    private void CleanupExpired()
    {
        var expiredCodes = _codes
            .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var code in expiredCodes)
        {
            _codes.TryRemove(code, out _);
        }
    }
}

public class AuthCodeEntry
{
    public string Code { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string? State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
