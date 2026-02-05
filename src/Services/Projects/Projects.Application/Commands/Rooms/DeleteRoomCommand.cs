using MediatR;

namespace Projects.Application.Commands.Rooms;

public sealed record DeleteRoomCommand(Guid ProjectId, Guid RoomId) : IRequest;
