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
    private readonly FallbackEventStore _fallbackStore;
    private readonly ILogger<ResilientEventPublisher> _logger;

    public const string AuditChannel = "domeo:audit:entities";
    public const string SessionChannel = "domeo:audit:sessions";
    public const string ErrorChannel = "domeo:logs:errors";

    public ResilientEventPublisher(
        IConnectionStateTracker stateTracker,
        FallbackEventStore fallbackStore,
        ILogger<ResilientEventPublisher> logger,
        IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
        _stateTracker = stateTracker;
        _fallbackStore = fallbackStore;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var channel = $"domeo:{typeof(TEvent).Name.ToLowerInvariant()}";
        var message = JsonSerializer.Serialize(@event);

        if (!_stateTracker.IsRedisAvailable || _redis is null)
        {
            _logger.LogDebug("Redis unavailable, storing event {EventType} to fallback", typeof(TEvent).Name);
            await _fallbackStore.StoreEventAsync(channel, message, cancellationToken);
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to publish event {EventType}: Redis connection lost, storing to fallback", typeof(TEvent).Name);
            _stateTracker.SetRedisAvailable(false);
            await _fallbackStore.StoreEventAsync(channel, message, cancellationToken);
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Failed to publish event {EventType}: Redis timeout, storing to fallback", typeof(TEvent).Name);
            await _fallbackStore.StoreEventAsync(channel, message, cancellationToken);
        }
    }

    public async Task PublishAuditAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        var message = JsonSerializer.Serialize(auditEvent);

        if (!_stateTracker.IsRedisAvailable || _redis is null)
        {
            _logger.LogDebug("Redis unavailable, storing audit event for {EntityType}/{EntityId} to fallback",
                auditEvent.EntityType, auditEvent.EntityId);
            await _fallbackStore.StoreEventAsync(AuditChannel, message, cancellationToken);
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(AuditChannel), message);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit event: Redis connection lost, storing to fallback");
            _stateTracker.SetRedisAvailable(false);
            await _fallbackStore.StoreEventAsync(AuditChannel, message, cancellationToken);
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit event: Redis timeout, storing to fallback");
            await _fallbackStore.StoreEventAsync(AuditChannel, message, cancellationToken);
        }
    }

    public async Task PublishSessionAsync(SessionEvent sessionEvent, CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var wrapper = new SessionEventWrapper
        {
            EventType = sessionEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(sessionEvent, sessionEvent.GetType(), options)
        };
        var message = JsonSerializer.Serialize(wrapper, options);

        if (!_stateTracker.IsRedisAvailable || _redis is null)
        {
            _logger.LogDebug("Redis unavailable, storing session event {EventType} for user {UserEmail} to fallback",
                sessionEvent.GetType().Name, sessionEvent.UserEmail);
            await _fallbackStore.StoreEventAsync(SessionChannel, message, cancellationToken);
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(SessionChannel), message);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to publish session event: Redis connection lost, storing to fallback");
            _stateTracker.SetRedisAvailable(false);
            await _fallbackStore.StoreEventAsync(SessionChannel, message, cancellationToken);
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Failed to publish session event: Redis timeout, storing to fallback");
            await _fallbackStore.StoreEventAsync(SessionChannel, message, cancellationToken);
        }
    }

    public async Task PublishErrorAsync(ApplicationErrorEvent errorEvent, CancellationToken cancellationToken = default)
    {
        var message = JsonSerializer.Serialize(errorEvent);

        if (!_stateTracker.IsRedisAvailable || _redis is null)
        {
            _logger.LogDebug("Redis unavailable, storing error event for {ServiceName} to fallback",
                errorEvent.ServiceName);
            await _fallbackStore.StoreEventAsync(ErrorChannel, message, cancellationToken);
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(ErrorChannel), message);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to publish error event: Redis connection lost, storing to fallback");
            _stateTracker.SetRedisAvailable(false);
            await _fallbackStore.StoreEventAsync(ErrorChannel, message, cancellationToken);
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Failed to publish error event: Redis timeout, storing to fallback");
            await _fallbackStore.StoreEventAsync(ErrorChannel, message, cancellationToken);
        }
    }
}
