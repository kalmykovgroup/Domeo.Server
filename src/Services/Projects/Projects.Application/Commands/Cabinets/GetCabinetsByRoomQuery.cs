using MediatR;
using Projects.Contracts.DTOs.Cabinets;

namespace Projects.Application.Commands.Cabinets;

public sealed record GetCabinetsByRoomQuery(Guid RoomId) : IRequest<List<CabinetDto>>;
