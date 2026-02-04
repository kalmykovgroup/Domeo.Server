using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Clients.Abstractions.Commands;

public sealed record RestoreClientCommand(Guid Id) : ICommand<ClientDto>;
