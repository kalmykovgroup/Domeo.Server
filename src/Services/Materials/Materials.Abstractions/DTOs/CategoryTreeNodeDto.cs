namespace Materials.Abstractions.DTOs;

public sealed class CategoryTreeNodeDto
{
    public string Id { get; init; } = string.Empty;
    public string? ParentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Level { get; init; }
    public int OrderIndex { get; init; }
    public bool IsActive { get; init; }
    public List<string> SupplierIds { get; init; } = [];
    public List<CategoryTreeNodeDto> Children { get; init; } = [];
}
