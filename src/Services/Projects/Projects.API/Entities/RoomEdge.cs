using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Projects.API.Entities;

public sealed class RoomEdge : Entity<Guid>
{
    public Guid RoomId { get; private set; }
    public Guid StartVertexId { get; private set; }
    public Guid EndVertexId { get; private set; }
    public int WallHeight { get; private set; } = 2700;
    public bool HasWindow { get; private set; }
    public bool HasDoor { get; private set; }
    public int OrderIndex { get; private set; }

    private RoomEdge() { }

    public static RoomEdge Create(
        Guid roomId,
        Guid startVertexId,
        Guid endVertexId,
        int wallHeight = 2700,
        bool hasWindow = false,
        bool hasDoor = false,
        int orderIndex = 0)
    {
        return new RoomEdge
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            StartVertexId = startVertexId,
            EndVertexId = endVertexId,
            WallHeight = wallHeight,
            HasWindow = hasWindow,
            HasDoor = hasDoor,
            OrderIndex = orderIndex
        };
    }

    public void Update(int wallHeight, bool hasWindow, bool hasDoor, int orderIndex)
    {
        WallHeight = wallHeight;
        HasWindow = hasWindow;
        HasDoor = hasDoor;
        OrderIndex = orderIndex;
    }
}
