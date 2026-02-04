using MediatR;

namespace Projects.Abstractions.Commands.Rooms;

public sealed record DeleteRoomCommand(Guid ProjectId, Guid RoomId) : IRequest;
