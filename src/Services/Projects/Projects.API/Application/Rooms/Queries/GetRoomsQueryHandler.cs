using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.Rooms;
using Projects.Abstractions.DTOs;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.Rooms.Queries;

public sealed class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, List<RoomDto>>
{
    private readonly ProjectsDbContext _dbContext;

    public GetRoomsQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = await _dbContext.Rooms
            .Where(r => r.ProjectId == request.ProjectId)
            .OrderBy(r => r.OrderIndex)
            .Select(r => new RoomDto(
                r.Id,
                r.ProjectId,
                r.Name,
                r.CeilingHeight,
                r.OrderIndex,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        return rooms;
    }
}
