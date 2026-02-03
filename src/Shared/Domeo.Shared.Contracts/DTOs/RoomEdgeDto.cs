namespace Domeo.Shared.Contracts.DTOs;

public sealed record RoomEdgeDto(
    Guid Id,
    Guid RoomId,
    Guid StartVertexId,
    Guid EndVertexId,
    int WallHeight,
    bool HasWindow,
    bool HasDoor,
    int OrderIndex);
