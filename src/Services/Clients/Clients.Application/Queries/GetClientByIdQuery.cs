using Clients.Contracts.DTOs;
using MediatR;

namespace Clients.Application.Queries;

public sealed record GetClientByIdQuery(Guid Id) : IRequest<ClientDto>;
