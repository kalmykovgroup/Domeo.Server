using MediatR;

namespace Projects.Abstractions.Commands.RoomEdges;

public sealed record DeleteRoomEdgeCommand(Guid RoomId, Guid EdgeId) : IRequest;
