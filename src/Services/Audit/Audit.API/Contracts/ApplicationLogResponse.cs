namespace Audit.API.Contracts;

public sealed record ApplicationLogResponse(
    Guid Id,
    string ServiceName,
    string Level,
    string Message,
    string? Exception,
    string? ExceptionType,
    string? Properties,
    string? RequestPath,
    Guid? UserId,
    string? CorrelationId,
    DateTime CreatedAt);
