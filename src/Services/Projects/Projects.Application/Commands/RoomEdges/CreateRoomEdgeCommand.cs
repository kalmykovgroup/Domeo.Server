using MediatR;

namespace Projects.Application.Commands.RoomEdges;

public sealed record CreateRoomEdgeCommand(
    Guid RoomId,
    Guid StartVertexId,
    Guid EndVertexId,
    int WallHeight,
    bool HasWindow,
    bool HasDoor,
    int OrderIndex) : IRequest<Guid>;
