using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Abstractions.Queries.AuditLogs;

public sealed record GetEntityHistoryQuery(
    string EntityType,
    string EntityId,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<AuditLogDto>>;
