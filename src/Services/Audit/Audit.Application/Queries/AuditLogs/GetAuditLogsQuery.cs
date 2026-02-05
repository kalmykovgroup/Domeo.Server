using Audit.Contracts.DTOs.AuditLogs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Application.Queries.AuditLogs;

public sealed record GetAuditLogsQuery(
    Guid? UserId,
    string? Action,
    string? EntityType,
    string? ServiceName,
    DateTime? From,
    DateTime? To,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<AuditLogDto>>;
