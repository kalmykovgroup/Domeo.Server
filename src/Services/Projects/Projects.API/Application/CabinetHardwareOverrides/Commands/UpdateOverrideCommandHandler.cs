using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.CabinetHardwareOverrides;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetHardwareOverrides.Commands;

public sealed class UpdateOverrideCommandHandler : IRequestHandler<UpdateOverrideCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public UpdateOverrideCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateOverrideCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.CabinetHardwareOverrides.FindAsync([request.Id], cancellationToken);

        if (entity is null)
            throw new NotFoundException("Override", request.Id);

        entity.Update(
            request.IsEnabled,
            request.ComponentId,
            request.Role,
            request.QuantityFormula,
            request.PositionXFormula,
            request.PositionYFormula,
            request.PositionZFormula,
            request.MaterialId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
