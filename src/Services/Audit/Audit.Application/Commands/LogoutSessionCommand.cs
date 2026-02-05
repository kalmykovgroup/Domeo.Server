using MediatR;

namespace Audit.Application.Commands;

public sealed record LogoutSessionCommand(Guid SessionId) : IRequest;
