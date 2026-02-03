namespace Projects.API.Contracts;

public sealed record UpdateRoomRequest(
    string Name,
    int CeilingHeight,
    int OrderIndex);
