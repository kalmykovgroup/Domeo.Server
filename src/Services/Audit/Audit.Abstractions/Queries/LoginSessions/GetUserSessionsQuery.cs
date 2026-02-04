using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Abstractions.Queries.LoginSessions;

public sealed record GetUserSessionsQuery(
    Guid UserId,
    bool? ActiveOnly,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<LoginSessionDto>>;
