namespace Projects.Contracts.DTOs.RoomVertices;

public sealed record CreateRoomVertexRequest(
    double X,
    double Y,
    int OrderIndex);
