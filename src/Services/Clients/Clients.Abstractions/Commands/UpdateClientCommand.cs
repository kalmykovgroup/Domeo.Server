using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Clients.Abstractions.Commands;

public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : ICommand<ClientDto>;
