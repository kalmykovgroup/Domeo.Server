using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Commands.Zones;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.Zones.Commands;

public sealed class UpdateZoneCommandHandler : IRequestHandler<UpdateZoneCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public UpdateZoneCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await _dbContext.Zones
            .FirstOrDefaultAsync(z => z.Id == request.ZoneId && z.EdgeId == request.EdgeId, cancellationToken);

        if (zone is null)
            throw new NotFoundException("Zone", request.ZoneId);

        zone.Update(request.Name, request.StartX, request.EndX);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
