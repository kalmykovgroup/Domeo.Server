namespace Modules.Contracts.DTOs.Categories;

public sealed class ModuleCategoryDto
{
    public string Id { get; }
    public string? ParentId { get; }
    public string Name { get; }
    public string? Description { get; }
    public int OrderIndex { get; }
    public bool IsActive { get; }
    public List<ModuleCategoryDto> Children { get; }

    public ModuleCategoryDto(
        string id,
        string? parentId,
        string name,
        string? description,
        int orderIndex,
        bool isActive,
        List<ModuleCategoryDto>? children = null)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        Description = description;
        OrderIndex = orderIndex;
        IsActive = isActive;
        Children = children ?? [];
    }
}
