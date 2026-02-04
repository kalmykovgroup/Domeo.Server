using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure.Resilience;

public sealed class RedisHealthMonitorService : BackgroundService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IConnectionStateTracker _stateTracker;
    private readonly ILogger<RedisHealthMonitorService> _logger;
    private readonly ResilienceOptions _options;

    private int _currentRetryDelayMs;
    private int _consecutiveFailures;

    public RedisHealthMonitorService(
        IConnectionStateTracker stateTracker,
        IOptions<ResilienceOptions> options,
        ILogger<RedisHealthMonitorService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
        _stateTracker = stateTracker;
        _logger = logger;
        _options = options.Value;
        _currentRetryDelayMs = _options.InitialRetryDelayMs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_redis is null)
        {
            _logger.LogInformation("Redis connection not configured. Health monitor will not run");
            _stateTracker.SetRedisAvailable(false);
            return;
        }

        _logger.LogInformation("Redis health monitor started");

        // Initial delay to let the application start
        await Task.Delay(1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var isAvailable = CheckRedisConnection();

                if (isAvailable)
                {
                    HandleSuccess();
                }
                else
                {
                    HandleFailure();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Redis health check failed");
                HandleFailure();
            }

            var delay = _stateTracker.IsRedisAvailable
                ? _options.HealthyCheckIntervalMs
                : _currentRetryDelayMs;

            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("Redis health monitor stopped");
    }

    private bool CheckRedisConnection()
    {
        if (_redis is null)
            return false;

        return _redis.IsConnected;
    }

    private void HandleSuccess()
    {
        if (!_stateTracker.IsRedisAvailable)
        {
            _logger.LogInformation("Redis connection restored");
        }

        _stateTracker.SetRedisAvailable(true);
        _consecutiveFailures = 0;
        _currentRetryDelayMs = _options.InitialRetryDelayMs;
    }

    private void HandleFailure()
    {
        _consecutiveFailures++;

        if (_stateTracker.IsRedisAvailable)
        {
            _logger.LogWarning("Redis connection lost");
        }

        _stateTracker.SetRedisAvailable(false);

        // Exponential backoff
        _currentRetryDelayMs = Math.Min(
            (int)(_currentRetryDelayMs * _options.BackoffMultiplier),
            _options.MaxRetryDelayMs);

        _logger.LogDebug(
            "Redis health check failed (attempt {Attempt}). Next retry in {Delay}ms",
            _consecutiveFailures,
            _currentRetryDelayMs);
    }
}
