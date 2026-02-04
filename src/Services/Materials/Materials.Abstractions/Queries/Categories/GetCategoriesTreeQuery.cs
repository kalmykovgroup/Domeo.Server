using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Categories;

public sealed record GetCategoriesTreeQuery(bool? ActiveOnly) : IRequest<List<CategoryTreeNodeDto>>;
