using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.CabinetParts;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetParts.Commands;

public sealed class UpdateCabinetPartCommandHandler : IRequestHandler<UpdateCabinetPartCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public UpdateCabinetPartCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateCabinetPartCommand request, CancellationToken cancellationToken)
    {
        var part = await _dbContext.CabinetParts.FindAsync([request.Id], cancellationToken);

        if (part is null)
            throw new NotFoundException("CabinetPart", request.Id);

        part.Update(
            request.ComponentId,
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

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
