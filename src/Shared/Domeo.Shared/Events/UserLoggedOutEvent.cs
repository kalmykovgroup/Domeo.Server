namespace Domeo.Shared.Events;

public sealed record UserLoggedOutEvent : SessionEvent
{
    public required Guid SessionId { get; init; }
}
