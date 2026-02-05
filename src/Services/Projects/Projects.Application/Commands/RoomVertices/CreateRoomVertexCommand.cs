using MediatR;

namespace Projects.Application.Commands.RoomVertices;

public sealed record CreateRoomVertexCommand(
    Guid RoomId,
    double X,
    double Y,
    int OrderIndex) : IRequest<Guid>;
