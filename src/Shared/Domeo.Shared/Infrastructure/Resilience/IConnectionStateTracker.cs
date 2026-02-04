namespace Domeo.Shared.Infrastructure.Resilience;

public interface IConnectionStateTracker
{
    /// <summary>
    /// Indicates whether the database connection is available
    /// </summary>
    bool IsDatabaseAvailable { get; }

    /// <summary>
    /// Indicates whether the Redis connection is available
    /// </summary>
    bool IsRedisAvailable { get; }

    /// <summary>
    /// Sets the database availability state
    /// </summary>
    void SetDatabaseAvailable(bool available);

    /// <summary>
    /// Sets the Redis availability state
    /// </summary>
    void SetRedisAvailable(bool available);

    /// <summary>
    /// Event raised when database availability changes
    /// </summary>
    event EventHandler<bool>? DatabaseAvailabilityChanged;

    /// <summary>
    /// Event raised when Redis availability changes
    /// </summary>
    event EventHandler<bool>? RedisAvailabilityChanged;
}
