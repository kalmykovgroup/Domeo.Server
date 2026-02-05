using Domeo.Shared.Domain;

namespace Projects.API.Entities;

public sealed class Cabinet : AuditableEntity<Guid>
{
    public Guid RoomId { get; private set; }
    public Guid? EdgeId { get; private set; }
    public Guid? ZoneId { get; private set; }
    public Guid? AssemblyId { get; private set; }
    public string? Name { get; private set; }
    public string PlacementType { get; private set; } = string.Empty;
    public string? FacadeType { get; private set; }
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public double Rotation { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }
    public double Depth { get; private set; }
    public decimal? CalculatedPrice { get; private set; }

    private Cabinet() { }

    public static Cabinet Create(
        Guid roomId,
        string placementType,
        double positionX,
        double positionY,
        double width,
        double height,
        double depth,
        string? name = null)
    {
        return new Cabinet
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            PlacementType = placementType,
            PositionX = positionX,
            PositionY = positionY,
            Width = width,
            Height = height,
            Depth = depth,
            Name = name
        };
    }

    public void UpdatePosition(double x, double y, double rotation)
    {
        PositionX = x;
        PositionY = y;
        Rotation = rotation;
    }

    public void UpdateDimensions(double width, double height, double depth)
    {
        Width = width;
        Height = height;
        Depth = depth;
    }

    public void SetAssembly(Guid? assemblyId) => AssemblyId = assemblyId;
    public void SetEdge(Guid? edgeId) => EdgeId = edgeId;
    public void SetZone(Guid? zoneId) => ZoneId = zoneId;
    public void SetFacadeType(string? facadeType) => FacadeType = facadeType;
    public void SetName(string? name) => Name = name;
    public void SetCalculatedPrice(decimal? price) => CalculatedPrice = price;
}
