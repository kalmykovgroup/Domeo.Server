namespace Audit.API.Contracts;

public sealed record AuditLogResponse(
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
    DateTime CreatedAt);
