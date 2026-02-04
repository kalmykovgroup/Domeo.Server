using Clients.Abstractions.DTOs;
using MediatR;

namespace Clients.Abstractions.Commands;

public sealed record RestoreClientCommand(Guid Id) : IRequest<ClientDto>;
