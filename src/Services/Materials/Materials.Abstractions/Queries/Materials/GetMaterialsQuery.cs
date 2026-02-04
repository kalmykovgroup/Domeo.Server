using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Materials;

public sealed record GetMaterialsQuery(string? CategoryId, bool? ActiveOnly) : IRequest<List<MaterialDto>>;
