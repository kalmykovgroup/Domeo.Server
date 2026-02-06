namespace Modules.Contracts.DTOs.Categories;

public sealed record CreateCategoryRequest(
    string Id,
    string Name,
    string? ParentId,
    string? Description,
    int OrderIndex);
