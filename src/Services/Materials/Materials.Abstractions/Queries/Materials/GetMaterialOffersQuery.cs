using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Materials;

public sealed record GetMaterialOffersQuery(string MaterialId) : IRequest<MaterialOffersDto>;
