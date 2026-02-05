using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Commands.Zones;
using Projects.Contracts.DTOs.Zones;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.Zones.Queries;

public sealed class GetZonesQueryHandler : IRequestHandler<GetZonesQuery, List<ZoneDto>>
{
    private readonly ProjectsDbContext _dbContext;

    public GetZonesQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ZoneDto>> Handle(GetZonesQuery request, CancellationToken cancellationToken)
    {
        var zones = await _dbContext.Zones
            .Where(z => z.EdgeId == request.EdgeId)
            .Select(z => new ZoneDto(
                z.Id,
                z.EdgeId,
                z.Name,
                z.Type.ToString(),
                z.StartX,
                z.EndX))
            .ToListAsync(cancellationToken);

        return zones;
    }
}
