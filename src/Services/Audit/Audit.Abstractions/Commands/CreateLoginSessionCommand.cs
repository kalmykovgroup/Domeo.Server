using Audit.Abstractions.DTOs;
using MediatR;

namespace Audit.Abstractions.Commands;

public sealed record CreateLoginSessionCommand(
    Guid UserId,
    string UserRole,
    string? IpAddress,
    string? UserAgent) : IRequest<LoginSessionDto>;
