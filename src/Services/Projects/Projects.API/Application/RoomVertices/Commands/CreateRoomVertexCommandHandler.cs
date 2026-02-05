using MediatR;
using Projects.Application.Commands.RoomVertices;
using Projects.Domain.Entities;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.RoomVertices.Commands;

public sealed class CreateRoomVertexCommandHandler : IRequestHandler<CreateRoomVertexCommand, Guid>
{
    private readonly ProjectsDbContext _dbContext;

    public CreateRoomVertexCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateRoomVertexCommand request, CancellationToken cancellationToken)
    {
        var vertex = RoomVertex.Create(request.RoomId, request.X, request.Y, request.OrderIndex);

        _dbContext.RoomVertices.Add(vertex);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return vertex.Id;
    }
}
