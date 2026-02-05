using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Brands;

public sealed record GetBrandsQuery(bool? ActiveOnly) : IRequest<List<BrandDto>>;
