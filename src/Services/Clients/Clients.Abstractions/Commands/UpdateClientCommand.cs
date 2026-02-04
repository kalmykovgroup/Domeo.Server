using Clients.Abstractions.DTOs;
using MediatR;

namespace Clients.Abstractions.Commands;

public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : IRequest<ClientDto>;
