using Domeo.Shared.Domain;
using Modules.Abstractions.Entities.Shared;

namespace Modules.Abstractions.Entities;

public sealed class Assembly : Entity<Guid>
{
    public string CategoryId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Dimensions Dimensions { get; private set; } = null!;
    public Constraints? Constraints { get; private set; }
    public Construction? Construction { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private Assembly() { }

    public static Assembly Create(
        string categoryId,
        string type,
        string name,
        Dimensions dimensions,
        Constraints? constraints = null,
        Construction? construction = null)
    {
        return new Assembly
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Type = type,
            Name = name,
            Dimensions = dimensions,
            Constraints = constraints,
            Construction = construction,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, Dimensions dimensions, Constraints? constraints, Construction? construction)
    {
        Name = name;
        Dimensions = dimensions;
        Constraints = constraints;
        Construction = construction;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
