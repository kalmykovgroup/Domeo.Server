using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Commands.CabinetParts;
using Projects.Domain.Entities;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetParts.Commands;

public sealed class CreateCabinetPartCommandHandler : IRequestHandler<CreateCabinetPartCommand, Guid>
{
    private readonly ProjectsDbContext _dbContext;

    public CreateCabinetPartCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateCabinetPartCommand request, CancellationToken cancellationToken)
    {
        var cabinetExists = await _dbContext.Cabinets.AnyAsync(c => c.Id == request.CabinetId, cancellationToken);
        if (!cabinetExists)
            throw new NotFoundException("Cabinet", request.CabinetId);

        var part = CabinetPart.Create(
            request.CabinetId,
            request.ComponentId,
            request.SourceAssemblyPartId,
            request.X,
            request.Y,
            request.Z,
            request.RotationX,
            request.RotationY,
            request.RotationZ,
            request.Shape,
            request.Condition,
            request.Quantity,
            request.QuantityFormula,
            request.SortOrder,
            request.IsEnabled,
            request.MaterialId,
            request.Provides);

        _dbContext.CabinetParts.Add(part);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return part.Id;
    }
}
