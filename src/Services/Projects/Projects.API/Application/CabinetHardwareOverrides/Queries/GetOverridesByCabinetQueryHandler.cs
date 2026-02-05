using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Commands.CabinetHardwareOverrides;
using Projects.Abstractions.DTOs;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetHardwareOverrides.Queries;

public sealed class GetOverridesByCabinetQueryHandler : IRequestHandler<GetOverridesByCabinetQuery, List<CabinetHardwareOverrideDto>>
{
    private readonly ProjectsDbContext _dbContext;

    public GetOverridesByCabinetQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CabinetHardwareOverrideDto>> Handle(GetOverridesByCabinetQuery request, CancellationToken cancellationToken)
    {
        var cabinetExists = await _dbContext.Cabinets.AnyAsync(c => c.Id == request.CabinetId, cancellationToken);
        if (!cabinetExists)
            throw new NotFoundException("Cabinet", request.CabinetId);

        var overrides = await _dbContext.CabinetHardwareOverrides
            .Where(o => o.CabinetId == request.CabinetId)
            .Select(o => new CabinetHardwareOverrideDto(
                o.Id,
                o.CabinetId,
                o.AssemblyPartId,
                o.ComponentId,
                o.Role,
                o.QuantityFormula,
                o.PositionXFormula,
                o.PositionYFormula,
                o.PositionZFormula,
                o.IsEnabled,
                o.MaterialId,
                o.CreatedAt,
                o.UpdatedAt ?? o.CreatedAt))
            .ToListAsync(cancellationToken);

        return overrides;
    }
}
