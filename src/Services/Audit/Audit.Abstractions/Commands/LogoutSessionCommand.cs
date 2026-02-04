using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Commands;

public sealed record LogoutSessionCommand(Guid SessionId) : ICommand;
