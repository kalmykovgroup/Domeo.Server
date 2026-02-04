using Domeo.Shared.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Domeo.Shared.Infrastructure.Redis;

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
        _logger.LogDebug("NoOp: Audit event for {EntityType}/{EntityId} would be published (Redis unavailable)",
            auditEvent.EntityType, auditEvent.EntityId);
        return Task.CompletedTask;
    }

    public Task PublishSessionAsync(SessionEvent sessionEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOp: Session event {EventType} for user {UserEmail} would be published (Redis unavailable)",
            sessionEvent.GetType().Name, sessionEvent.UserEmail);
        return Task.CompletedTask;
    }

    public Task PublishErrorAsync(ApplicationErrorEvent errorEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOp: Error event from {ServiceName} would be published (Redis unavailable)",
            errorEvent.ServiceName);
        return Task.CompletedTask;
    }
}
