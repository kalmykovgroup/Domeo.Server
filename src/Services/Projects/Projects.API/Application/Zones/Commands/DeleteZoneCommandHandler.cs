using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Commands.Zones;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.Zones.Commands;

public sealed class DeleteZoneCommandHandler : IRequestHandler<DeleteZoneCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public DeleteZoneCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await _dbContext.Zones
            .FirstOrDefaultAsync(z => z.Id == request.ZoneId && z.EdgeId == request.EdgeId, cancellationToken);

        if (zone is null)
            throw new NotFoundException("Zone", request.ZoneId);

        _dbContext.Zones.Remove(zone);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
