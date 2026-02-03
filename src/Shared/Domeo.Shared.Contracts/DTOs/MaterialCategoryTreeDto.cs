namespace Domeo.Shared.Contracts.DTOs;

public sealed class MaterialCategoryTreeDto
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Level { get; init; }
    public int OrderIndex { get; init; }
    public bool IsActive { get; init; }
    public List<MaterialCategoryTreeDto> Children { get; init; } = [];

    public MaterialCategoryTreeDto() { }

    public MaterialCategoryTreeDto(
        Guid id,
        Guid? parentId,
        string name,
        int level,
        int orderIndex,
        bool isActive)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        Level = level;
        OrderIndex = orderIndex;
        IsActive = isActive;
    }
}
