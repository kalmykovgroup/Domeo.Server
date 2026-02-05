using MediatR;
using Modules.Contracts.DTOs.Categories;

namespace Modules.Application.Queries.Categories;

public sealed record GetCategoriesQuery(bool? ActiveOnly) : IRequest<List<ModuleCategoryDto>>;
