namespace Projects.Contracts.DTOs.RoomEdges;

public sealed record UpdateRoomEdgeRequest(
    int WallHeight,
    bool HasWindow,
    bool HasDoor,
    int OrderIndex);
