namespace Audit.Abstractions.DTOs;

public sealed record ApplicationLogDto(
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
