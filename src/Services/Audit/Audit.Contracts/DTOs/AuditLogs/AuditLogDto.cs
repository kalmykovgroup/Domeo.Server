namespace Audit.Contracts.DTOs.AuditLogs;

public sealed record AuditLogDto(
    Guid Id,
    Guid UserId,
    string Action,
    string EntityType,
    string EntityId,
    string ServiceName,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    DateTime CreatedAt);
