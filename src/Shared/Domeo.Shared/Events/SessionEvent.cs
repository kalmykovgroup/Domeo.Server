namespace Domeo.Shared.Events;

public abstract record SessionEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public required string UserRole { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
