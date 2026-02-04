using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Materials;

public sealed record GetMaterialByIdQuery(string Id) : IRequest<MaterialDto>;
