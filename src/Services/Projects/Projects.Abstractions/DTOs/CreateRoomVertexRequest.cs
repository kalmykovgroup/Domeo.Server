namespace Projects.Abstractions.DTOs;

public sealed record CreateRoomVertexRequest(
    double X,
    double Y,
    int OrderIndex);
