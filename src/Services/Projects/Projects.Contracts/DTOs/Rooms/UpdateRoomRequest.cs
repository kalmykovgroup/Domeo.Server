namespace Projects.Contracts.DTOs.Rooms;

public sealed record UpdateRoomRequest(
    string Name,
    int CeilingHeight,
    int OrderIndex);
