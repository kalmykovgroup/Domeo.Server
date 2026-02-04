namespace Domeo.Shared.Events;

public sealed record ApplicationErrorEvent : IntegrationEvent
{
    public required string ServiceName { get; init; }
    public required string Level { get; init; }
    public required string Message { get; init; }
    public string? Exception { get; init; }
    public string? ExceptionType { get; init; }
    public string? Properties { get; init; }
    public string? RequestPath { get; init; }
    public Guid? UserId { get; init; }
}
