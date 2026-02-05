using Audit.Contracts.DTOs.AuditLogs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Application.Queries.AuditLogs;

public sealed record GetEntityHistoryQuery(
    string EntityType,
    string EntityId,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<AuditLogDto>>;
