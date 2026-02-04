using Audit.Abstractions.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Commands;

public sealed record CreateLoginSessionCommand(
    Guid UserId,
    string UserEmail,
    string? UserName,
    string UserRole,
    string? IpAddress,
    string? UserAgent) : ICommand<LoginSessionDto>;
