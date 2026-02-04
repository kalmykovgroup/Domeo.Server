using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Categories;

public sealed record GetCategoriesTreeQuery(bool? ActiveOnly) : IRequest<List<ModuleCategoryTreeDto>>;
