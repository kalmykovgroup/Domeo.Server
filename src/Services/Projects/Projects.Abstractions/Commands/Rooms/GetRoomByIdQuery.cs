using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.Rooms;

public sealed record GetRoomByIdQuery(Guid ProjectId, Guid RoomId) : IRequest<RoomDto>;
