using Domeo.Shared.Contracts.Events;

namespace Domeo.Shared.Infrastructure.Redis;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    Task PublishAuditAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
