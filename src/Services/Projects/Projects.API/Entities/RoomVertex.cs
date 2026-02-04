using Domeo.Shared.Domain;

namespace Projects.API.Entities;

public sealed class RoomVertex : Entity<Guid>
{
    public Guid RoomId { get; private set; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public int OrderIndex { get; private set; }

    private RoomVertex() { }

    public static RoomVertex Create(
        Guid roomId,
        double x,
        double y,
        int orderIndex)
    {
        return new RoomVertex
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            X = x,
            Y = y,
            OrderIndex = orderIndex
        };
    }

    public void Update(double x, double y, int orderIndex)
    {
        X = x;
        Y = y;
        OrderIndex = orderIndex;
    }
}
