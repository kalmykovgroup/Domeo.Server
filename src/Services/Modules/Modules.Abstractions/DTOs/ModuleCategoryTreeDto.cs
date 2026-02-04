namespace Modules.Abstractions.DTOs;

public sealed class ModuleCategoryTreeDto
{
    public string Id { get; }
    public string? ParentId { get; }
    public string Name { get; }
    public string? Description { get; }
    public int OrderIndex { get; }
    public bool IsActive { get; }
    public List<ModuleCategoryTreeDto> Children { get; } = [];

    public ModuleCategoryTreeDto(
        string id,
        string? parentId,
        string name,
        string? description,
        int orderIndex,
        bool isActive)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        Description = description;
        OrderIndex = orderIndex;
        IsActive = isActive;
    }
}
