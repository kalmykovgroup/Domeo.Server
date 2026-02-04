using System.Text.Json;
using Domeo.Shared.Contracts.Events;
using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure.Redis;

public sealed class ResilientEventPublisher : IEventPublisher
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IConnectionStateTracker _stateTracker;
    private readonly ILogger<ResilientEventPublisher> _logger;

    public const string AuditChannel = "domeo:audit:entities";
    public const string SessionChannel = "domeo:audit:sessions";

    public ResilientEventPublisher(
        IConnectionStateTracker stateTracker,
        ILogger<ResilientEventPublisher> logger,
        IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
        _stateTracker = stateTracker;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (!_stateTracker.IsRedisAvailable || _redis is null)
        {
            _logger.LogDebug("Skipping event {EventType}: Redis unavailable", typeof(TEvent).Name);
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            var channel = $"domeo:{typeof(TEvent).Name.ToLowerInvariant()}";
            var message = JsonSerializer.Serialize(@event);

            await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to publish event {EventType}: Redis connection lost", typeof(TEvent).Name);
            _stateTracker.SetRedisAvailable(false);
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Failed to publish event {EventType}: Redis timeout", typeof(TEvent).Name);
        }
    }

    public async Task PublishAuditAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        if (!_stateTracker.IsRedisAvailable || _redis is null)
        {
            _logger.LogDebug("Skipping audit event for {EntityType}/{EntityId}: Redis unavailable",
                auditEvent.EntityType, auditEvent.EntityId);
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            var message = JsonSerializer.Serialize(auditEvent);

            await subscriber.PublishAsync(RedisChannel.Literal(AuditChannel), message);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit event: Redis connection lost");
            _stateTracker.SetRedisAvailable(false);
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit event: Redis timeout");
        }
    }

    public async Task PublishSessionAsync(SessionEvent sessionEvent, CancellationToken cancellationToken = default)
    {
        if (!_stateTracker.IsRedisAvailable || _redis is null)
        {
            _logger.LogDebug("Skipping session event {EventType} for user {UserEmail}: Redis unavailable",
                sessionEvent.GetType().Name, sessionEvent.UserEmail);
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var wrapper = new SessionEventWrapper
            {
                EventType = sessionEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(sessionEvent, sessionEvent.GetType(), options)
            };
            var message = JsonSerializer.Serialize(wrapper, options);

            await subscriber.PublishAsync(RedisChannel.Literal(SessionChannel), message);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to publish session event: Redis connection lost");
            _stateTracker.SetRedisAvailable(false);
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Failed to publish session event: Redis timeout");
        }
    }
}
