using Domeo.Shared.Domain;
using Modules.Domain.Entities.Shared;

namespace Modules.Domain.Entities;

public sealed class AssemblyPart : Entity<Guid>
{
    public Guid AssemblyId { get; private set; }
    public Guid ComponentId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string? LengthExpr { get; private set; }
    public string? WidthExpr { get; private set; }
    public string? X { get; private set; }
    public string? Y { get; private set; }
    public string? Z { get; private set; }
    public double RotationX { get; private set; }
    public double RotationY { get; private set; }
    public double RotationZ { get; private set; }
    public string? Condition { get; private set; }
    public List<ShapeSegment>? Shape { get; private set; }
    public int Quantity { get; private set; } = 1;
    public string? QuantityFormula { get; private set; }
    public int SortOrder { get; private set; }

    private AssemblyPart() { }

    public static AssemblyPart Create(
        Guid assemblyId,
        Guid componentId,
        string role,
        string? lengthExpr = null,
        string? widthExpr = null,
        string? x = null,
        string? y = null,
        string? z = null,
        double rotationX = 0,
        double rotationY = 0,
        double rotationZ = 0,
        string? condition = null,
        int quantity = 1,
        string? quantityFormula = null,
        int sortOrder = 0,
        List<ShapeSegment>? shape = null)
    {
        return new AssemblyPart
        {
            Id = Guid.NewGuid(),
            AssemblyId = assemblyId,
            ComponentId = componentId,
            Role = role,
            LengthExpr = lengthExpr,
            WidthExpr = widthExpr,
            X = x,
            Y = y,
            Z = z,
            RotationX = rotationX,
            RotationY = rotationY,
            RotationZ = rotationZ,
            Condition = condition,
            Shape = shape,
            Quantity = quantity,
            QuantityFormula = quantityFormula,
            SortOrder = sortOrder
        };
    }

    public void Update(
        Guid componentId,
        string role,
        string? lengthExpr,
        string? widthExpr,
        string? x,
        string? y,
        string? z,
        double rotationX,
        double rotationY,
        double rotationZ,
        string? condition,
        int quantity,
        string? quantityFormula,
        int sortOrder,
        List<ShapeSegment>? shape = null)
    {
        ComponentId = componentId;
        Role = role;
        LengthExpr = lengthExpr;
        WidthExpr = widthExpr;
        X = x;
        Y = y;
        Z = z;
        RotationX = rotationX;
        RotationY = rotationY;
        RotationZ = rotationZ;
        Condition = condition;
        Shape = shape;
        Quantity = quantity;
        QuantityFormula = quantityFormula;
        SortOrder = sortOrder;
    }
}
