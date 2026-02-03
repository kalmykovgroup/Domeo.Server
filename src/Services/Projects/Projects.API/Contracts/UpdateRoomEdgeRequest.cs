namespace Projects.API.Contracts;

public sealed record UpdateRoomEdgeRequest(
    int WallHeight,
    bool HasWindow,
    bool HasDoor,
    int OrderIndex);
