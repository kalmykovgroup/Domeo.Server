namespace Domeo.Shared.Contracts.DTOs;

public sealed record MaterialCategoryDto(
    Guid Id,
    Guid? ParentId,
    string Name,
    int Level,
    int OrderIndex,
    bool IsActive);
