using MediatR;
using Projects.Contracts.DTOs.Rooms;

namespace Projects.Application.Commands.Rooms;

public sealed record GetRoomByIdQuery(Guid ProjectId, Guid RoomId) : IRequest<RoomDto>;
