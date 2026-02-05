namespace MockSupplier.API.Models;

public sealed class MaterialAttributeValue
{
    public string MaterialId { get; set; } = string.Empty;
    public string AttributeId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
