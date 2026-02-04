using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.RoomVertices;

public sealed record GetRoomVerticesQuery(Guid RoomId) : IRequest<List<RoomVertexDto>>;
