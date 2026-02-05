using MediatR;
using Projects.Contracts.DTOs.RoomEdges;

namespace Projects.Application.Commands.RoomEdges;

public sealed record GetRoomEdgesQuery(Guid RoomId) : IRequest<List<RoomEdgeDto>>;
