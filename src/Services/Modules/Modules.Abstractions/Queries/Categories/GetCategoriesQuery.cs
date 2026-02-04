using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Modules.Abstractions.Queries.Categories;

public sealed record GetCategoriesQuery(bool? ActiveOnly) : IQuery<List<ModuleCategoryDto>>;
