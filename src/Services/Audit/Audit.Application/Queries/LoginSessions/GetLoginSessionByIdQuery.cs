using Audit.Contracts.DTOs.LoginSessions;
using MediatR;

namespace Audit.Application.Queries.LoginSessions;

public sealed record GetLoginSessionByIdQuery(Guid Id) : IRequest<LoginSessionDto>;
