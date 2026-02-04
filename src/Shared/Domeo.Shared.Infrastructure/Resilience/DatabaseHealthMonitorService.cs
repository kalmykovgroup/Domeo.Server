using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Domeo.Shared.Infrastructure.Resilience;

public sealed class DatabaseHealthMonitorService<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionStateTracker _stateTracker;
    private readonly ILogger<DatabaseHealthMonitorService<TDbContext>> _logger;
    private readonly ResilienceOptions _options;

    private int _currentRetryDelayMs;
    private int _consecutiveFailures;

    public DatabaseHealthMonitorService(
        IServiceScopeFactory scopeFactory,
        IConnectionStateTracker stateTracker,
        IOptions<ResilienceOptions> options,
        ILogger<DatabaseHealthMonitorService<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _stateTracker = stateTracker;
        _logger = logger;
        _options = options.Value;
        _currentRetryDelayMs = _options.InitialRetryDelayMs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database health monitor started for {DbContext}", typeof(TDbContext).Name);

        // Initial delay to let the application start
        await Task.Delay(1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var isAvailable = await CheckDatabaseConnectionAsync(stoppingToken);

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
                _logger.LogDebug(ex, "Database health check failed");
                HandleFailure();
            }

            var delay = _stateTracker.IsDatabaseAvailable
                ? _options.HealthyCheckIntervalMs
                : _currentRetryDelayMs;

            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("Database health monitor stopped for {DbContext}", typeof(TDbContext).Name);
    }

    private async Task<bool> CheckDatabaseConnectionAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        return await dbContext.Database.CanConnectAsync(cts.Token);
    }

    private void HandleSuccess()
    {
        if (!_stateTracker.IsDatabaseAvailable)
        {
            _logger.LogInformation("Database connection restored for {DbContext}", typeof(TDbContext).Name);
        }

        _stateTracker.SetDatabaseAvailable(true);
        _consecutiveFailures = 0;
        _currentRetryDelayMs = _options.InitialRetryDelayMs;
    }

    private void HandleFailure()
    {
        _consecutiveFailures++;

        if (_stateTracker.IsDatabaseAvailable)
        {
            _logger.LogWarning("Database connection lost for {DbContext}", typeof(TDbContext).Name);
        }

        _stateTracker.SetDatabaseAvailable(false);

        // Exponential backoff
        _currentRetryDelayMs = Math.Min(
            (int)(_currentRetryDelayMs * _options.BackoffMultiplier),
            _options.MaxRetryDelayMs);

        _logger.LogDebug(
            "Database health check failed (attempt {Attempt}). Next retry in {Delay}ms",
            _consecutiveFailures,
            _currentRetryDelayMs);
    }
}
