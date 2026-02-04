namespace Projects.Abstractions.DTOs;

public sealed record UpdateRoomRequest(
    string Name,
    int CeilingHeight,
    int OrderIndex);
