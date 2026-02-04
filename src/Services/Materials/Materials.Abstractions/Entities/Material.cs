using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Materials.Abstractions.Entities;

public sealed class Material : Entity<Guid>
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Unit { get; private set; } = "sqm";
    public string? Color { get; private set; }
    public string? TextureUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Material() { }

    public static Material Create(
        Guid id,
        Guid categoryId,
        string name,
        string? description = null,
        string unit = "sqm",
        string? color = null,
        string? textureUrl = null)
    {
        return new Material
        {
            Id = id,
            CategoryId = categoryId,
            Name = name,
            Description = description,
            Unit = unit,
            Color = color,
            TextureUrl = textureUrl,
            IsActive = true
        };
    }

    public void Update(string name, string? description, string? color, string? textureUrl)
    {
        Name = name;
        Description = description;
        Color = color;
        TextureUrl = textureUrl;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
