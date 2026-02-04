namespace MockSupplier.API.Models;

public sealed class Category
{
    public string Id { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
}
