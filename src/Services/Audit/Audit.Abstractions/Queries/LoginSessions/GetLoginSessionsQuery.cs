using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.Abstractions.Queries.LoginSessions;

public sealed record GetLoginSessionsQuery(
    Guid? UserId,
    bool? ActiveOnly,
    DateTime? From,
    DateTime? To,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<LoginSessionDto>>;
