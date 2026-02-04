namespace Domeo.Shared.Events;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    Task PublishAuditAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    Task PublishSessionAsync(SessionEvent sessionEvent, CancellationToken cancellationToken = default);

    Task PublishErrorAsync(ApplicationErrorEvent errorEvent, CancellationToken cancellationToken = default);
}
