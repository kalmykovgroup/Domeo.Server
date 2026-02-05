namespace Projects.Contracts.DTOs.Rooms;

public sealed record CreateRoomRequest(
    string Name,
    int CeilingHeight = 2700,
    int OrderIndex = 0);
