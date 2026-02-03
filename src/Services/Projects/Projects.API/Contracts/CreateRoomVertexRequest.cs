namespace Projects.API.Contracts;

public sealed record CreateRoomVertexRequest(
    double X,
    double Y,
    int OrderIndex);
