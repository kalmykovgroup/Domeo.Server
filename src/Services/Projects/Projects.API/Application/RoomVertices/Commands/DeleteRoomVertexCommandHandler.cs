using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.RoomVertices;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.RoomVertices.Commands;

public sealed class DeleteRoomVertexCommandHandler : IRequestHandler<DeleteRoomVertexCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public DeleteRoomVertexCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteRoomVertexCommand request, CancellationToken cancellationToken)
    {
        var vertex = await _dbContext.RoomVertices
            .FirstOrDefaultAsync(v => v.Id == request.VertexId && v.RoomId == request.RoomId, cancellationToken);

        if (vertex is null)
            throw new NotFoundException("Vertex", request.VertexId);

        _dbContext.RoomVertices.Remove(vertex);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
