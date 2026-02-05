using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Materials;

public sealed record GetMaterialsQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? BrandId = null,
    string? SupplierId = null,
    Dictionary<string, string>? Attributes = null) : IRequest<List<MaterialDto>>;
