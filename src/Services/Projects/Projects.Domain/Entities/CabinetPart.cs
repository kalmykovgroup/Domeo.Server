using Domeo.Shared.Domain;
using Modules.Domain.Entities.Shared;

namespace Projects.Domain.Entities;

public sealed class CabinetPart : AuditableEntity<Guid>
{
    public Guid CabinetId { get; private set; }
    public Guid? SourceAssemblyPartId { get; private set; }
    public Guid ComponentId { get; private set; }

    public string? X { get; private set; }
    public string? Y { get; private set; }
    public string? Z { get; private set; }
    public double RotationX { get; private set; }
    public double RotationY { get; private set; }
    public double RotationZ { get; private set; }
    public List<ShapeSegment>? Shape { get; private set; }

    public string? Condition { get; private set; }
    public int Quantity { get; private set; } = 1;
    public string? QuantityFormula { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public Guid? MaterialId { get; private set; }
    public Dictionary<string, string>? Provides { get; private set; }

    private CabinetPart() { }

    public static CabinetPart Create(
        Guid cabinetId,
        Guid componentId,
        Guid? sourceAssemblyPartId = null,
        string? x = null,
        string? y = null,
        string? z = null,
        double rotationX = 0,
        double rotationY = 0,
        double rotationZ = 0,
        List<ShapeSegment>? shape = null,
        string? condition = null,
        int quantity = 1,
        string? quantityFormula = null,
        int sortOrder = 0,
        bool isEnabled = true,
        Guid? materialId = null,
        Dictionary<string, string>? provides = null)
    {
        return new CabinetPart
        {
            Id = Guid.NewGuid(),
            CabinetId = cabinetId,
            ComponentId = componentId,
            SourceAssemblyPartId = sourceAssemblyPartId,
            X = x,
            Y = y,
            Z = z,
            RotationX = rotationX,
            RotationY = rotationY,
            RotationZ = rotationZ,
            Shape = shape,
            Condition = condition,
            Quantity = quantity,
            QuantityFormula = quantityFormula,
            SortOrder = sortOrder,
            IsEnabled = isEnabled,
            MaterialId = materialId,
            Provides = provides
        };
    }

    public void Update(
        Guid componentId,
        string? x,
        string? y,
        string? z,
        double rotationX,
        double rotationY,
        double rotationZ,
        List<ShapeSegment>? shape,
        string? condition,
        int quantity,
        string? quantityFormula,
        int sortOrder,
        bool isEnabled,
        Guid? materialId,
        Dictionary<string, string>? provides = null)
    {
        ComponentId = componentId;
        X = x;
        Y = y;
        Z = z;
        RotationX = rotationX;
        RotationY = rotationY;
        RotationZ = rotationZ;
        Shape = shape;
        Condition = condition;
        Quantity = quantity;
        QuantityFormula = quantityFormula;
        SortOrder = sortOrder;
        IsEnabled = isEnabled;
        MaterialId = materialId;
        Provides = provides;
    }
}
