namespace Domeo.Shared.Contracts.DTOs;

public sealed record AuditLogSummaryDto(
    Guid Id,
    string Action,
    string EntityType,
    string EntityId,
    string ServiceName,
    DateTime Timestamp);
