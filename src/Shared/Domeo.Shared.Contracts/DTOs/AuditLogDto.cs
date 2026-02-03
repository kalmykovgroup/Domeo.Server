namespace Domeo.Shared.Contracts.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string Action,
    string EntityType,
    string EntityId,
    string ServiceName,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    DateTime Timestamp,
    string CorrelationId);
