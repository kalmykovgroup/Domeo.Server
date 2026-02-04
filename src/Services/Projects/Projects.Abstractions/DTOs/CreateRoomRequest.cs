namespace Projects.Abstractions.DTOs;

public sealed record CreateRoomRequest(
    string Name,
    int CeilingHeight = 2700,
    int OrderIndex = 0);
