namespace MockSupplier.API.Models;

public sealed class Material
{
    public string Id { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? BrandId { get; set; }
    public string? BrandName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = "sqm";
    public string? Color { get; set; }
    public string? TextureUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
