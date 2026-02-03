namespace Domeo.Shared.Contracts.Events;

public sealed record UserUpdatedEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
}
