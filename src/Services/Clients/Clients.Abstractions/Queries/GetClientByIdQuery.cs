using Clients.Abstractions.DTOs;
using MediatR;

namespace Clients.Abstractions.Queries;

public sealed record GetClientByIdQuery(Guid Id) : IRequest<ClientDto>;
