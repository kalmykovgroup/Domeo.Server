using Domeo.Shared.Kernel.Application.Abstractions;
using Materials.Abstractions.DTOs;

namespace Materials.Abstractions.Queries.Materials;

public sealed record GetMaterialByIdQuery(string Id) : IQuery<MaterialDto>;
