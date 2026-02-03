namespace Projects.API.Contracts;

public sealed record CreateRoomRequest(
    string Name,
    int CeilingHeight = 2700,
    int OrderIndex = 0);
