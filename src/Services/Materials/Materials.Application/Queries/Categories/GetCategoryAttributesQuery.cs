using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Categories;

public sealed record GetCategoryAttributesQuery(string CategoryId) : IRequest<List<CategoryAttributeDto>>;
