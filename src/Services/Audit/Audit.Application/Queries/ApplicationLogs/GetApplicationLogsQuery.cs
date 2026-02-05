using Audit.Contracts.DTOs.ApplicationLogs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Application.Queries.ApplicationLogs;

public sealed record GetApplicationLogsQuery(
    string? Level,
    string? ServiceName,
    Guid? UserId,
    DateTime? From,
    DateTime? To,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<ApplicationLogDto>>;
