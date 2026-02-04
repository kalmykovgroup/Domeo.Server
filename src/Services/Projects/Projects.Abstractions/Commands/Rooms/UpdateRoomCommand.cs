using MediatR;

namespace Projects.Abstractions.Commands.Rooms;

public sealed record UpdateRoomCommand(
    Guid ProjectId,
    Guid RoomId,
    string Name,
    int CeilingHeight,
    int OrderIndex) : IRequest;
