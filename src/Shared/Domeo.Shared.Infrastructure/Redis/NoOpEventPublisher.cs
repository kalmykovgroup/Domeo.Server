using Domeo.Shared.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Domeo.Shared.Infrastructure.Redis;

/// <summary>
/// No-op implementation of IEventPublisher for development when Redis is unavailable.
/// </summary>
public sealed class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;

    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        _logger.LogDebug("NoOp: Event {EventType} would be published (Redis unavailable)", typeof(TEvent).Name);
        return Task.CompletedTask;
    }

    public Task PublishAuditAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOp: Audit event would be published (Redis unavailable)");
        return Task.CompletedTask;
    }
}
