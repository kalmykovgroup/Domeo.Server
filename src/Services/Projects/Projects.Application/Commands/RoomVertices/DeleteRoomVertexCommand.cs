using MediatR;

namespace Projects.Application.Commands.RoomVertices;

public sealed record DeleteRoomVertexCommand(Guid RoomId, Guid VertexId) : IRequest;
