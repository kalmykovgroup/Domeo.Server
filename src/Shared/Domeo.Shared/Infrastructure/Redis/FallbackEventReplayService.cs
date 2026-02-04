using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure.Redis;

/// <summary>
/// Background service that replays fallback events to Redis when it becomes available.
/// </summary>
public sealed class FallbackEventReplayService : BackgroundService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IConnectionStateTracker _stateTracker;
    private readonly FallbackEventStore _fallbackStore;
    private readonly ILogger<FallbackEventReplayService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(6);
    private DateTime _lastCleanup = DateTime.MinValue;

    public FallbackEventReplayService(
        IConnectionStateTracker stateTracker,
        FallbackEventStore fallbackStore,
        ILogger<FallbackEventReplayService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
        _stateTracker = stateTracker;
        _fallbackStore = fallbackStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FallbackEventReplayService started. Monitoring {Directory}",
            _fallbackStore.FallbackDirectory);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CheckInterval, stoppingToken);

                // Cleanup old processed files periodically
                if (DateTime.UtcNow - _lastCleanup > CleanupInterval)
                {
                    _fallbackStore.CleanupOldFiles();
                    _lastCleanup = DateTime.UtcNow;
                }

                // Only replay if Redis is available
                if (!_stateTracker.IsRedisAvailable || _redis is null)
                {
                    continue;
                }

                await ReplayPendingEventsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FallbackEventReplayService");
            }
        }

        _logger.LogInformation("FallbackEventReplayService stopped");
    }

    private async Task ReplayPendingEventsAsync(CancellationToken stoppingToken)
    {
        var pendingFiles = _fallbackStore.GetPendingFiles().ToList();

        if (pendingFiles.Count == 0)
            return;

        _logger.LogInformation("Found {Count} pending fallback files to replay", pendingFiles.Count);

        foreach (var filePath in pendingFiles)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            // Check Redis is still available before processing each file
            if (!_stateTracker.IsRedisAvailable)
            {
                _logger.LogWarning("Redis became unavailable during replay. Stopping.");
                break;
            }

            await ReplayFileAsync(filePath, stoppingToken);
        }
    }

    private async Task ReplayFileAsync(string filePath, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Replaying fallback events from {FilePath}", filePath);

        var successCount = 0;
        var failCount = 0;

        try
        {
            var subscriber = _redis!.GetSubscriber();

            await foreach (var entry in _fallbackStore.ReadEventsAsync(filePath))
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    await subscriber.PublishAsync(
                        RedisChannel.Literal(entry.Channel),
                        entry.Message);
                    successCount++;
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogWarning(ex, "Redis connection lost during replay");
                    _stateTracker.SetRedisAvailable(false);
                    failCount++;
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to replay event to channel {Channel}", entry.Channel);
                    failCount++;
                }
            }

            // Only mark as processed if all events were replayed successfully
            if (failCount == 0 && !stoppingToken.IsCancellationRequested)
            {
                _fallbackStore.MarkFileAsProcessed(filePath);
                _logger.LogInformation("Successfully replayed {Count} events from {FilePath}",
                    successCount, filePath);
            }
            else
            {
                _logger.LogWarning("Partial replay: {Success} succeeded, {Failed} failed from {FilePath}",
                    successCount, failCount, filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replay file {FilePath}", filePath);
        }
    }
}
