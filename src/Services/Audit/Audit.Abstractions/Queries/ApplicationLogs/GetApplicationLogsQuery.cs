using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Queries.ApplicationLogs;

public sealed record GetApplicationLogsQuery(
    string? Level,
    string? ServiceName,
    Guid? UserId,
    DateTime? From,
    DateTime? To,
    int? Page,
    int? PageSize) : IQuery<PaginatedResponse<ApplicationLogDto>>;
