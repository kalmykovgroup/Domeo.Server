using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Materials;

public sealed record GetMaterialOffersQuery(string MaterialId) : IRequest<MaterialOffersDto>;
