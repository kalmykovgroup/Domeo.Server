using Domeo.Shared.Domain;

namespace Projects.API.Entities;

public sealed class Zone : Entity<Guid>
{
    public Guid EdgeId { get; private set; }
    public string? Name { get; private set; }
    public ZoneType Type { get; private set; }
    public double StartX { get; private set; }
    public double EndX { get; private set; }

    private Zone() { }

    public static Zone Create(
        Guid edgeId,
        ZoneType type,
        double startX,
        double endX,
        string? name = null)
    {
        return new Zone
        {
            Id = Guid.NewGuid(),
            EdgeId = edgeId,
            Type = type,
            StartX = startX,
            EndX = endX,
            Name = name
        };
    }

    public void Update(string? name, double startX, double endX)
    {
        Name = name;
        StartX = startX;
        EndX = endX;
    }
}
