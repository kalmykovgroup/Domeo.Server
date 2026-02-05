using MediatR;

namespace Clients.Application.Commands;

public sealed record DeleteClientCommand(Guid Id) : IRequest;
