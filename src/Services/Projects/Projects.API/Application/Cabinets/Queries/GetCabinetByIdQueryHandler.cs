using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.Cabinets;
using Projects.Contracts.DTOs.Cabinets;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.Cabinets.Queries;

public sealed class GetCabinetByIdQueryHandler : IRequestHandler<GetCabinetByIdQuery, CabinetDto>
{
    private readonly ProjectsDbContext _dbContext;

    public GetCabinetByIdQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CabinetDto> Handle(GetCabinetByIdQuery request, CancellationToken cancellationToken)
    {
        var cabinet = await _dbContext.Cabinets.FindAsync([request.Id], cancellationToken);

        if (cabinet is null)
            throw new NotFoundException("Cabinet", request.Id);

        return new CabinetDto(
            cabinet.Id,
            cabinet.RoomId,
            cabinet.EdgeId,
            cabinet.ZoneId,
            cabinet.AssemblyId,
            cabinet.Name,
            cabinet.PlacementType,
            cabinet.FacadeType,
            cabinet.PositionX,
            cabinet.PositionY,
            cabinet.Rotation,
            cabinet.ParameterOverrides,
            cabinet.CalculatedPrice,
            cabinet.CreatedAt);
    }
}
