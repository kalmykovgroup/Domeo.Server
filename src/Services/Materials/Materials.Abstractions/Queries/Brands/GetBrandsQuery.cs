using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Brands;

public sealed record GetBrandsQuery(bool? ActiveOnly) : IRequest<List<BrandDto>>;
