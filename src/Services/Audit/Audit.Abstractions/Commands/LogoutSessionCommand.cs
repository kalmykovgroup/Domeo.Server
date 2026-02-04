using MediatR;

namespace Audit.Abstractions.Commands;

public sealed record LogoutSessionCommand(Guid SessionId) : IRequest;
