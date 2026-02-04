using Clients.Abstractions.DTOs;
using MediatR;

namespace Clients.Abstractions.Commands;

public sealed record CreateClientCommand(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : IRequest<ClientDto>;
