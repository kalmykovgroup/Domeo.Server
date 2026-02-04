using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.Cabinets;
using Projects.Abstractions.DTOs;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.Cabinets.Queries;

public sealed class GetCabinetsByRoomQueryHandler : IRequestHandler<GetCabinetsByRoomQuery, List<CabinetDto>>
{
    private readonly ProjectsDbContext _dbContext;

    public GetCabinetsByRoomQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CabinetDto>> Handle(GetCabinetsByRoomQuery request, CancellationToken cancellationToken)
    {
        var cabinets = await _dbContext.Cabinets
            .Where(c => c.RoomId == request.RoomId)
            .Select(c => new CabinetDto(
                c.Id,
                c.RoomId,
                c.EdgeId,
                c.ZoneId,
                c.ModuleTypeId,
                c.Name,
                c.PlacementType,
                c.FacadeType,
                c.PositionX,
                c.PositionY,
                c.Rotation,
                c.Width,
                c.Height,
                c.Depth,
                c.CalculatedPrice,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return cabinets;
    }
}
