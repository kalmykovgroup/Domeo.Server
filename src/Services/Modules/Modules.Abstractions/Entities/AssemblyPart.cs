using Domeo.Shared.Domain;
using Modules.Abstractions.Entities.Shared;

namespace Modules.Abstractions.Entities;

public sealed class AssemblyPart : Entity<Guid>
{
    public Guid AssemblyId { get; private set; }
    public Guid ComponentId { get; private set; }
    public PartRole Role { get; private set; }
    public DynamicSize? Length { get; private set; }
    public DynamicSize? Width { get; private set; }
    public Placement Placement { get; private set; } = null!;
    public int Quantity { get; private set; } = 1;
    public string? QuantityFormula { get; private set; }
    public int SortOrder { get; private set; }

    private AssemblyPart() { }

    public static AssemblyPart Create(
        Guid assemblyId,
        Guid componentId,
        PartRole role,
        Placement placement,
        DynamicSize? length = null,
        DynamicSize? width = null,
        int quantity = 1,
        string? quantityFormula = null,
        int sortOrder = 0)
    {
        return new AssemblyPart
        {
            Id = Guid.NewGuid(),
            AssemblyId = assemblyId,
            ComponentId = componentId,
            Role = role,
            Placement = placement,
            Length = length,
            Width = width,
            Quantity = quantity,
            QuantityFormula = quantityFormula,
            SortOrder = sortOrder
        };
    }
}
