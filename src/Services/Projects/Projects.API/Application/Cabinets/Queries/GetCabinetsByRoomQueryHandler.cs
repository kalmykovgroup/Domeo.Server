using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Commands.Cabinets;
using Projects.Contracts.DTOs.Cabinets;
using Projects.Infrastructure.Persistence;

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
                c.AssemblyId,
                c.Name,
                c.PlacementType,
                c.FacadeType,
                c.PositionX,
                c.PositionY,
                c.Rotation,
                c.ParameterOverrides,
                c.CalculatedPrice,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return cabinets;
    }
}
