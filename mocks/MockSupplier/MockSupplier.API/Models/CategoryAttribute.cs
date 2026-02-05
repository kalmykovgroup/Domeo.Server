namespace MockSupplier.API.Models;

public sealed class CategoryAttribute
{
    public string Id { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string? Unit { get; set; }
    public List<string>? EnumValues { get; set; }
}
