using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.Rooms;

public sealed record GetRoomsQuery(Guid ProjectId) : IRequest<List<RoomDto>>;
