using MediatR;
using Projects.Application.Commands.RoomEdges;
using Projects.Domain.Entities;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.RoomEdges.Commands;

public sealed class CreateRoomEdgeCommandHandler : IRequestHandler<CreateRoomEdgeCommand, Guid>
{
    private readonly ProjectsDbContext _dbContext;

    public CreateRoomEdgeCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateRoomEdgeCommand request, CancellationToken cancellationToken)
    {
        var edge = RoomEdge.Create(
            request.RoomId,
            request.StartVertexId,
            request.EndVertexId,
            request.WallHeight,
            request.HasWindow,
            request.HasDoor,
            request.OrderIndex);

        _dbContext.RoomEdges.Add(edge);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return edge.Id;
    }
}
