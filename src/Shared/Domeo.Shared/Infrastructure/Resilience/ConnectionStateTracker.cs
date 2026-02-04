using Microsoft.Extensions.Logging;

namespace Domeo.Shared.Infrastructure.Resilience;

public sealed class ConnectionStateTracker : IConnectionStateTracker
{
    private readonly ILogger<ConnectionStateTracker> _logger;
    private volatile bool _isDatabaseAvailable = true;
    private volatile bool _isRedisAvailable = true;

    public ConnectionStateTracker(ILogger<ConnectionStateTracker> logger)
    {
        _logger = logger;
    }

    public bool IsDatabaseAvailable => _isDatabaseAvailable;

    public bool IsRedisAvailable => _isRedisAvailable;

    public event EventHandler<bool>? DatabaseAvailabilityChanged;
    public event EventHandler<bool>? RedisAvailabilityChanged;

    public void SetDatabaseAvailable(bool available)
    {
        if (_isDatabaseAvailable == available)
            return;

        _isDatabaseAvailable = available;

        if (available)
        {
            _logger.LogInformation("Database connection restored");
        }
        else
        {
            _logger.LogWarning("Database connection lost");
        }

        DatabaseAvailabilityChanged?.Invoke(this, available);
    }

    public void SetRedisAvailable(bool available)
    {
        if (_isRedisAvailable == available)
            return;

        _isRedisAvailable = available;

        if (available)
        {
            _logger.LogInformation("Redis connection restored");
        }
        else
        {
            _logger.LogWarning("Redis connection lost");
        }

        RedisAvailabilityChanged?.Invoke(this, available);
    }
}
