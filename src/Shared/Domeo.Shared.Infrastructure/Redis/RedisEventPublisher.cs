using System.Text.Json;
using Domeo.Shared.Contracts.Events;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure.Redis;

public sealed class RedisEventPublisher : IEventPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private const string AuditChannel = "domeo:audit:events";

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
}
