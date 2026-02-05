using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Abstractions.Commands.Cabinets;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.Cabinets.Commands;

public sealed class UpdateCabinetCommandHandler : IRequestHandler<UpdateCabinetCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public UpdateCabinetCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateCabinetCommand request, CancellationToken cancellationToken)
    {
        var cabinet = await _dbContext.Cabinets.FindAsync([request.Id], cancellationToken);

        if (cabinet is null)
            throw new NotFoundException("Cabinet", request.Id);

        cabinet.UpdatePosition(request.PositionX, request.PositionY, request.Rotation);
        cabinet.UpdateDimensions(request.Width, request.Height, request.Depth);
        cabinet.SetName(request.Name);
        cabinet.SetEdge(request.EdgeId);
        cabinet.SetZone(request.ZoneId);
        cabinet.SetAssembly(request.AssemblyId);
        cabinet.SetFacadeType(request.FacadeType);
        cabinet.SetCalculatedPrice(request.CalculatedPrice);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
