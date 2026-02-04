using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Categories;

public sealed record GetCategoriesQuery(bool? ActiveOnly) : IRequest<List<ModuleCategoryDto>>;
