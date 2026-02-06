namespace Modules.Contracts.DTOs.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    int OrderIndex);
