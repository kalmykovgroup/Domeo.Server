namespace Domeo.Shared.Contracts.DTOs;

public sealed record RoomVertexDto(
    Guid Id,
    Guid RoomId,
    double X,
    double Y,
    int OrderIndex);
