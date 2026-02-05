using MediatR;
using Projects.Contracts.DTOs.Rooms;

namespace Projects.Application.Commands.Rooms;

public sealed record GetRoomsQuery(Guid ProjectId) : IRequest<List<RoomDto>>;
