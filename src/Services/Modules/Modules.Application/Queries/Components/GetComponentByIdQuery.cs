using MediatR;
using Modules.Contracts.DTOs.Components;

namespace Modules.Application.Queries.Components;

public sealed record GetComponentByIdQuery(Guid Id) : IRequest<ComponentDto>;
