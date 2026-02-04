namespace Projects.Abstractions.DTOs;

public sealed record UpdateRoomEdgeRequest(
    int WallHeight,
    bool HasWindow,
    bool HasDoor,
    int OrderIndex);
