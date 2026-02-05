using Clients.Contracts.DTOs;
using MediatR;

namespace Clients.Application.Commands;

public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : IRequest<ClientDto>;
