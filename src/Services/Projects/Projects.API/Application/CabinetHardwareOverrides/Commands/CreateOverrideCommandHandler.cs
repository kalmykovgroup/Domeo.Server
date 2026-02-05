using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.CabinetHardwareOverrides;
using Projects.API.Entities;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetHardwareOverrides.Commands;

public sealed class CreateOverrideCommandHandler : IRequestHandler<CreateOverrideCommand, Guid>
{
    private readonly ProjectsDbContext _dbContext;

    public CreateOverrideCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateOverrideCommand request, CancellationToken cancellationToken)
    {
        var cabinetExists = await _dbContext.Cabinets.AnyAsync(c => c.Id == request.CabinetId, cancellationToken);
        if (!cabinetExists)
            throw new NotFoundException("Cabinet", request.CabinetId);

        var existingOverride = await _dbContext.CabinetHardwareOverrides
            .AnyAsync(o => o.CabinetId == request.CabinetId && o.AssemblyPartId == request.AssemblyPartId, cancellationToken);

        if (existingOverride)
            throw new ConflictException($"Override for cabinet {request.CabinetId} and assembly_part {request.AssemblyPartId} already exists");

        var entity = CabinetHardwareOverride.Create(
            request.CabinetId,
            request.AssemblyPartId,
            request.IsEnabled,
            request.ComponentId,
            request.Role,
            request.QuantityFormula,
            request.PositionXFormula,
            request.PositionYFormula,
            request.PositionZFormula,
            request.MaterialId);

        _dbContext.CabinetHardwareOverrides.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
