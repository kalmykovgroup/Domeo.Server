namespace Projects.Abstractions.DTOs;

public sealed record RoomVertexDto(
    Guid Id,
    Guid RoomId,
    double X,
    double Y,
    int OrderIndex);
