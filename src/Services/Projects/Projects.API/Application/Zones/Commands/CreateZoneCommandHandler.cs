using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.Zones;
using Projects.Domain.Entities;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.Zones.Commands;

public sealed class CreateZoneCommandHandler : IRequestHandler<CreateZoneCommand, Guid>
{
    private readonly ProjectsDbContext _dbContext;

    public CreateZoneCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateZoneCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ZoneType>(request.Type, true, out var zoneType))
            throw new DomeoValidationException("Type", "Invalid zone type");

        var zone = Zone.Create(request.EdgeId, zoneType, request.StartX, request.EndX, request.Name);

        _dbContext.Zones.Add(zone);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return zone.Id;
    }
}
