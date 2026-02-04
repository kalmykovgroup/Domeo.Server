using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.Cabinets;

public sealed record GetCabinetsByRoomQuery(Guid RoomId) : IRequest<List<CabinetDto>>;
