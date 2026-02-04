using Domeo.Shared.Kernel.Application.Abstractions;
using Materials.Abstractions.DTOs;

namespace Materials.Abstractions.Queries.Categories;

public sealed record GetCategoriesTreeQuery(bool? ActiveOnly) : IQuery<List<CategoryTreeNodeDto>>;
