namespace Modules.Abstractions.DTOs;

public sealed record ModuleCategoryDto(
    string Id,
    string? ParentId,
    string Name,
    string? Description,
    int OrderIndex,
    bool IsActive);
