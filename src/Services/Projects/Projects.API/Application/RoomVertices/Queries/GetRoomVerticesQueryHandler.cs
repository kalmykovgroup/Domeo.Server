using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.RoomVertices;
using Projects.Abstractions.DTOs;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.RoomVertices.Queries;

public sealed class GetRoomVerticesQueryHandler : IRequestHandler<GetRoomVerticesQuery, List<RoomVertexDto>>
{
    private readonly ProjectsDbContext _dbContext;

    public GetRoomVerticesQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomVertexDto>> Handle(GetRoomVerticesQuery request, CancellationToken cancellationToken)
    {
        var vertices = await _dbContext.RoomVertices
            .Where(v => v.RoomId == request.RoomId)
            .OrderBy(v => v.OrderIndex)
            .Select(v => new RoomVertexDto(v.Id, v.RoomId, v.X, v.Y, v.OrderIndex))
            .ToListAsync(cancellationToken);

        return vertices;
    }
}
