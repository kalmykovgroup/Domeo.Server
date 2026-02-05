using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Categories;

public sealed record GetCategoryAttributesQuery(string CategoryId) : IRequest<List<CategoryAttributeDto>>;
