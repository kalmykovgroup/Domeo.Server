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
            .AnyAsync(o => o.CabinetId == request.CabinetId && o.ModuleHardwareId == request.ModuleHardwareId, cancellationToken);

        if (existingOverride)
            throw new ConflictException($"Override for cabinet {request.CabinetId} and module_hardware {request.ModuleHardwareId} already exists");

        var entity = CabinetHardwareOverride.Create(
            request.CabinetId,
            request.ModuleHardwareId,
            request.IsEnabled,
            request.HardwareId,
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
