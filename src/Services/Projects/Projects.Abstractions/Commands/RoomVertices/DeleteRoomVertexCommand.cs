using MediatR;

namespace Projects.Abstractions.Commands.RoomVertices;

public sealed record DeleteRoomVertexCommand(Guid RoomId, Guid VertexId) : IRequest;
