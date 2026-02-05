using MediatR;

namespace Projects.Application.Commands.RoomEdges;

public sealed record DeleteRoomEdgeCommand(Guid RoomId, Guid EdgeId) : IRequest;
