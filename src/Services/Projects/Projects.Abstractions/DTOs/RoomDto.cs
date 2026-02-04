namespace Projects.Abstractions.DTOs;

public sealed record RoomDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    int CeilingHeight,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
