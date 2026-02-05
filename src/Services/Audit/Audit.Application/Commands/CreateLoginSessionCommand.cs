using Audit.Contracts.DTOs.LoginSessions;
using MediatR;

namespace Audit.Application.Commands;

public sealed record CreateLoginSessionCommand(
    Guid UserId,
    string UserRole,
    string? IpAddress,
    string? UserAgent) : IRequest<LoginSessionDto>;
