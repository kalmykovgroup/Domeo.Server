using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Clients.Abstractions.Commands;

public sealed record CreateClientCommand(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : ICommand<ClientDto>;
