using MediatR;
using Modules.Contracts.DTOs.Categories;

namespace Modules.Application.Queries.Categories;

public sealed record GetCategoriesTreeQuery(bool? ActiveOnly) : IRequest<List<ModuleCategoryTreeDto>>;
