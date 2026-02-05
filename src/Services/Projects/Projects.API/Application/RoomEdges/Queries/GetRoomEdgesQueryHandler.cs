using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Commands.RoomEdges;
using Projects.Contracts.DTOs.RoomEdges;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.RoomEdges.Queries;

public sealed class GetRoomEdgesQueryHandler : IRequestHandler<GetRoomEdgesQuery, List<RoomEdgeDto>>
{
    private readonly ProjectsDbContext _dbContext;

    public GetRoomEdgesQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomEdgeDto>> Handle(GetRoomEdgesQuery request, CancellationToken cancellationToken)
    {
        var edges = await _dbContext.RoomEdges
            .Where(e => e.RoomId == request.RoomId)
            .OrderBy(e => e.OrderIndex)
            .Select(e => new RoomEdgeDto(
                e.Id,
                e.RoomId,
                e.StartVertexId,
                e.EndVertexId,
                e.WallHeight,
                e.HasWindow,
                e.HasDoor,
                e.OrderIndex))
            .ToListAsync(cancellationToken);

        return edges;
    }
}
