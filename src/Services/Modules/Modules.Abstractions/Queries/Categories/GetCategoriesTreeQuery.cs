using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Modules.Abstractions.Queries.Categories;

public sealed record GetCategoriesTreeQuery(bool? ActiveOnly) : IQuery<List<ModuleCategoryTreeDto>>;
