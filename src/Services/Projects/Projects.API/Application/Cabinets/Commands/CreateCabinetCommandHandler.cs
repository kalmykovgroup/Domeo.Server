using MediatR;
using Projects.Application.Commands.Cabinets;
using Projects.Domain.Entities;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.Cabinets.Commands;

public sealed class CreateCabinetCommandHandler : IRequestHandler<CreateCabinetCommand, Guid>
{
    private readonly ProjectsDbContext _dbContext;

    public CreateCabinetCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateCabinetCommand request, CancellationToken cancellationToken)
    {
        var cabinet = Cabinet.Create(
            request.RoomId,
            request.PlacementType,
            request.PositionX,
            request.PositionY,
            request.ParameterOverrides,
            request.Name);

        if (request.EdgeId.HasValue)
            cabinet.SetEdge(request.EdgeId);
        if (request.ZoneId.HasValue)
            cabinet.SetZone(request.ZoneId);
        if (request.AssemblyId.HasValue)
            cabinet.SetAssembly(request.AssemblyId);
        if (request.FacadeType is not null)
            cabinet.SetFacadeType(request.FacadeType);
        if (request.Rotation != 0)
            cabinet.UpdatePosition(request.PositionX, request.PositionY, request.Rotation);

        _dbContext.Cabinets.Add(cabinet);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (request.Parts is { Count: > 0 })
        {
            foreach (var partReq in request.Parts)
            {
                var part = CabinetPart.Create(
                    cabinet.Id,
                    partReq.ComponentId,
                    partReq.SourceAssemblyPartId,
                    partReq.X,
                    partReq.Y,
                    partReq.Z,
                    partReq.RotationX,
                    partReq.RotationY,
                    partReq.RotationZ,
                    partReq.Shape,
                    partReq.Condition,
                    partReq.Quantity,
                    partReq.QuantityFormula,
                    partReq.SortOrder,
                    partReq.IsEnabled,
                    partReq.MaterialId,
                    partReq.Provides);

                _dbContext.CabinetParts.Add(part);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return cabinet.Id;
    }
}
