using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Categories;

public sealed record GetCategoriesTreeQuery(bool? ActiveOnly) : IRequest<List<CategoryTreeNodeDto>>;
