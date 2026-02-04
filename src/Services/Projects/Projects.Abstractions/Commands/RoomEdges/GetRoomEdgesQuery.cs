using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.RoomEdges;

public sealed record GetRoomEdgesQuery(Guid RoomId) : IRequest<List<RoomEdgeDto>>;
