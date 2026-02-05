namespace Projects.Contracts.DTOs.RoomEdges;

public sealed record RoomEdgeDto(
    Guid Id,
    Guid RoomId,
    Guid StartVertexId,
    Guid EndVertexId,
    int WallHeight,
    bool HasWindow,
    bool HasDoor,
    int OrderIndex);
