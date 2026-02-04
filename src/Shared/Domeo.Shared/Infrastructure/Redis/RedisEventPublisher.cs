using System.Text.Json;
using Domeo.Shared.Events;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure.Redis;

public sealed class RedisEventPublisher : IEventPublisher
{
    private readonly IConnectionMultiplexer _redis;

    public const string AuditChannel = "domeo:audit:entities";
    public const string SessionChannel = "domeo:audit:sessions";
    public const string ErrorChannel = "domeo:logs:errors";

    public RedisEventPublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var subscriber = _redis.GetSubscriber();
        var channel = $"domeo:{typeof(TEvent).Name.ToLowerInvariant()}";
        var message = JsonSerializer.Serialize(@event);

        await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
    }

    public async Task PublishAuditAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        var subscriber = _redis.GetSubscriber();
        var message = JsonSerializer.Serialize(auditEvent);

        await subscriber.PublishAsync(RedisChannel.Literal(AuditChannel), message);
    }

    public async Task PublishSessionAsync(SessionEvent sessionEvent, CancellationToken cancellationToken = default)
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

    public async Task PublishErrorAsync(ApplicationErrorEvent errorEvent, CancellationToken cancellationToken = default)
    {
        var subscriber = _redis.GetSubscriber();
        var message = JsonSerializer.Serialize(errorEvent);

        await subscriber.PublishAsync(RedisChannel.Literal(ErrorChannel), message);
    }
}

public sealed class SessionEventWrapper
{
    public required string EventType { get; init; }
    public required string Payload { get; init; }
}
