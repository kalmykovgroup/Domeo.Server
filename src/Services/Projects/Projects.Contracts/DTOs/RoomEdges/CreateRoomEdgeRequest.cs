namespace Projects.Contracts.DTOs.RoomEdges;

public sealed record CreateRoomEdgeRequest(
    Guid StartVertexId,
    Guid EndVertexId,
    int WallHeight = 2700,
    bool HasWindow = false,
    bool HasDoor = false,
    int OrderIndex = 0);
