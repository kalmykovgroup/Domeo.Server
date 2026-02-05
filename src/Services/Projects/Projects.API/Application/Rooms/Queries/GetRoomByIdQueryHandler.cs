using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Commands.Rooms;
using Projects.Contracts.DTOs.Rooms;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.Rooms.Queries;

public sealed class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    private readonly ProjectsDbContext _dbContext;

    public GetRoomByIdQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.ProjectId == request.ProjectId, cancellationToken);

        if (room is null)
            throw new NotFoundException("Room", request.RoomId);

        return new RoomDto(
            room.Id,
            room.ProjectId,
            room.Name,
            room.CeilingHeight,
            room.OrderIndex,
            room.CreatedAt,
            room.UpdatedAt);
    }
}
