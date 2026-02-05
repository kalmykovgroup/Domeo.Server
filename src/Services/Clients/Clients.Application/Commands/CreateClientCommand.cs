using Clients.Contracts.DTOs;
using MediatR;

namespace Clients.Application.Commands;

public sealed record CreateClientCommand(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : IRequest<ClientDto>;
