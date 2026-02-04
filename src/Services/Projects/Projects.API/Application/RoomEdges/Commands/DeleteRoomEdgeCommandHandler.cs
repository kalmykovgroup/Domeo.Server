using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.RoomEdges;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.RoomEdges.Commands;

public sealed class DeleteRoomEdgeCommandHandler : IRequestHandler<DeleteRoomEdgeCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public DeleteRoomEdgeCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteRoomEdgeCommand request, CancellationToken cancellationToken)
    {
        var edge = await _dbContext.RoomEdges
            .FirstOrDefaultAsync(e => e.Id == request.EdgeId && e.RoomId == request.RoomId, cancellationToken);

        if (edge is null)
            throw new NotFoundException("Edge", request.EdgeId);

        _dbContext.RoomEdges.Remove(edge);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
