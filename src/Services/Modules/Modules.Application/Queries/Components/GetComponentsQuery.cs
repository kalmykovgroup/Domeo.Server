using MediatR;
using Modules.Contracts.DTOs.Components;

namespace Modules.Application.Queries.Components;

public sealed record GetComponentsQuery(string? Tag, bool? ActiveOnly) : IRequest<List<ComponentDto>>;
