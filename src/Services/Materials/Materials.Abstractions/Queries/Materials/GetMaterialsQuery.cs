using Domeo.Shared.Kernel.Application.Abstractions;
using Materials.Abstractions.DTOs;

namespace Materials.Abstractions.Queries.Materials;

public sealed record GetMaterialsQuery(string? CategoryId, bool? ActiveOnly) : IQuery<List<MaterialDto>>;
