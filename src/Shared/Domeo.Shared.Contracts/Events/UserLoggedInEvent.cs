namespace Domeo.Shared.Contracts.Events;

public sealed record UserLoggedInEvent : SessionEvent
{
    public Guid SessionId { get; init; } = Guid.NewGuid();
}
