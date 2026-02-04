using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Abstractions.Commands.CabinetHardwareOverrides;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetHardwareOverrides.Commands;

public sealed class DeleteOverrideCommandHandler : IRequestHandler<DeleteOverrideCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public DeleteOverrideCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteOverrideCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.CabinetHardwareOverrides.FindAsync([request.Id], cancellationToken);

        if (entity is null)
            throw new NotFoundException("Override", request.Id);

        _dbContext.CabinetHardwareOverrides.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
