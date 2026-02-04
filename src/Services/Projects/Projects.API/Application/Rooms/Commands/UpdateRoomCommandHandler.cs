using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.Rooms;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.Rooms.Commands;

public sealed class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public UpdateRoomCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.ProjectId == request.ProjectId, cancellationToken);

        if (room is null)
            throw new NotFoundException("Room", request.RoomId);

        room.Update(request.Name, request.CeilingHeight, request.OrderIndex);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
