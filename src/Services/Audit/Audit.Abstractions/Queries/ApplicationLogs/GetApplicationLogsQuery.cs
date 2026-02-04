using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Abstractions.Queries.ApplicationLogs;

public sealed record GetApplicationLogsQuery(
    string? Level,
    string? ServiceName,
    Guid? UserId,
    DateTime? From,
    DateTime? To,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<ApplicationLogDto>>;
