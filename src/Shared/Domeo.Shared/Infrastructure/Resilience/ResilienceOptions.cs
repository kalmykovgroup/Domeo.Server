namespace Domeo.Shared.Infrastructure.Resilience;

public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    /// <summary>
    /// Initial retry delay in milliseconds (default: 1000ms = 1 sec)
    /// </summary>
    public int InitialRetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum retry delay in milliseconds (default: 5000ms = 5 sec)
    /// </summary>
    public int MaxRetryDelayMs { get; set; } = 5000;

    /// <summary>
    /// Backoff multiplier for exponential backoff (default: 1.5)
    /// </summary>
    public double BackoffMultiplier { get; set; } = 1.5;

    /// <summary>
    /// Health check interval when connection is healthy (default: 15000ms = 15 sec)
    /// </summary>
    public int HealthyCheckIntervalMs { get; set; } = 15000;

    /// <summary>
    /// Health check interval when connection is unhealthy (default: 2000ms = 2 sec)
    /// </summary>
    public int UnhealthyCheckIntervalMs { get; set; } = 2000;

    /// <summary>
    /// Allow application to start without database connection (default: true)
    /// </summary>
    public bool AllowStartWithoutDatabase { get; set; } = true;

    /// <summary>
    /// Allow application to start without Redis connection (default: true)
    /// </summary>
    public bool AllowStartWithoutRedis { get; set; } = true;
}
