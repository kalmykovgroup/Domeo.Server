using MediatR;
using Projects.Abstractions.Commands.Rooms;
using Projects.API.Entities;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.Rooms.Commands;

public sealed class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Guid>
{
    private readonly ProjectsDbContext _dbContext;

    public CreateRoomCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = Room.Create(
            request.ProjectId,
            request.Name,
            request.CeilingHeight,
            request.OrderIndex);

        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return room.Id;
    }
}
