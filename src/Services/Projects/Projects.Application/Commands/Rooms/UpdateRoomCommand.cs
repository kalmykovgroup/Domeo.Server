using MediatR;

namespace Projects.Application.Commands.Rooms;

public sealed record UpdateRoomCommand(
    Guid ProjectId,
    Guid RoomId,
    string Name,
    int CeilingHeight,
    int OrderIndex) : IRequest;
