using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Components;

public sealed record GetComponentsQuery(string? Tag, bool? ActiveOnly) : IRequest<List<ComponentDto>>;
