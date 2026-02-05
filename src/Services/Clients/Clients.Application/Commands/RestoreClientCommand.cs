using Clients.Contracts.DTOs;
using MediatR;

namespace Clients.Application.Commands;

public sealed record RestoreClientCommand(Guid Id) : IRequest<ClientDto>;
