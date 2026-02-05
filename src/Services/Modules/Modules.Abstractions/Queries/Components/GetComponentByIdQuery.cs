using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Components;

public sealed record GetComponentByIdQuery(Guid Id) : IRequest<ComponentDto>;
