using MediatR;
using Projects.Contracts.DTOs.RoomVertices;

namespace Projects.Application.Commands.RoomVertices;

public sealed record GetRoomVerticesQuery(Guid RoomId) : IRequest<List<RoomVertexDto>>;
