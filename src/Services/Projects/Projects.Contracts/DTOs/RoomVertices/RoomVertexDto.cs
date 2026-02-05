namespace Projects.Contracts.DTOs.RoomVertices;

public sealed record RoomVertexDto(
    Guid Id,
    Guid RoomId,
    double X,
    double Y,
    int OrderIndex);
