using MediatR;

namespace Projects.Abstractions.Commands.Rooms;

public sealed record CreateRoomCommand(
    Guid ProjectId,
    string Name,
    int CeilingHeight = 2700,
    int OrderIndex = 0) : IRequest<Guid>;
