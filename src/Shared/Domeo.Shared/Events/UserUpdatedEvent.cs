namespace Domeo.Shared.Events;

public sealed record UserUpdatedEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public string? Email { get; init; }
    public string? Role { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
