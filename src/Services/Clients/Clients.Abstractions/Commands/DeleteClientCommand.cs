using MediatR;

namespace Clients.Abstractions.Commands;

public sealed record DeleteClientCommand(Guid Id) : IRequest;
