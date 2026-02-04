using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.Rooms;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.Rooms.Commands;

public sealed class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public DeleteRoomCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.ProjectId == request.ProjectId, cancellationToken);

        if (room is null)
            throw new NotFoundException("Room", request.RoomId);

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
