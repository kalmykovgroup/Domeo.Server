namespace Domeo.Shared.Events;

public sealed record AuditEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public required string Action { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string ServiceName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string? IpAddress { get; init; }
}
