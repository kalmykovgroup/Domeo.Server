using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Queries.AuditLogs;

public sealed record GetEntityHistoryQuery(
    string EntityType,
    string EntityId,
    int? Page,
    int? PageSize) : IQuery<PaginatedResponse<AuditLogDto>>;
