using Audit.Abstractions.DTOs;
using MediatR;

namespace Audit.Abstractions.Queries.LoginSessions;

public sealed record GetLoginSessionByIdQuery(Guid Id) : IRequest<LoginSessionDto>;
