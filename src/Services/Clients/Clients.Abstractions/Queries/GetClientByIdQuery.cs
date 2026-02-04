using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Clients.Abstractions.Queries;

public sealed record GetClientByIdQuery(Guid Id) : IQuery<ClientDto>;
