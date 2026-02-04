namespace Domeo.Shared.Contracts.Events;

public abstract record SessionEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public required string UserEmail { get; init; }
    public string? UserName { get; init; }
    public required string UserRole { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
