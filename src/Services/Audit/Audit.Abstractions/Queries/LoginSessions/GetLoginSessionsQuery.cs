using Audit.Abstractions.DTOs;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Queries.LoginSessions;

public sealed record GetLoginSessionsQuery(
    Guid? UserId,
    bool? ActiveOnly,
    DateTime? From,
    DateTime? To,
    int? Page,
    int? PageSize) : IQuery<PaginatedResponse<LoginSessionDto>>;
