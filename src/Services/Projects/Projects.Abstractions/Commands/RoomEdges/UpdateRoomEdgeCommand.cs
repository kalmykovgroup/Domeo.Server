using MediatR;

namespace Projects.Abstractions.Commands.RoomEdges;

public sealed record UpdateRoomEdgeCommand(
    Guid RoomId,
    Guid EdgeId,
    int WallHeight,
    bool HasWindow,
    bool HasDoor,
    int OrderIndex) : IRequest;
