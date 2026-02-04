namespace Domeo.Shared.Events;

public sealed record UserCreatedEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
