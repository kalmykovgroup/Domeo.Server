using Audit.Abstractions.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Queries.LoginSessions;

public sealed record GetLoginSessionByIdQuery(Guid Id) : IQuery<LoginSessionDto>;
