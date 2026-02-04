using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Queries.LoginSessions;

public sealed record GetUserSessionsQuery(
    Guid UserId,
    bool? ActiveOnly,
    int? Page,
    int? PageSize) : IQuery<PaginatedResponse<LoginSessionDto>>;
