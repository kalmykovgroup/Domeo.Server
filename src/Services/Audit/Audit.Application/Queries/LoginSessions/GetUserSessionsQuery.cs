using Audit.Contracts.DTOs.LoginSessions;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Application.Queries.LoginSessions;

public sealed record GetUserSessionsQuery(
    Guid UserId,
    bool? ActiveOnly,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<LoginSessionDto>>;
