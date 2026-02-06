using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Queries.CabinetParts;
using Projects.Contracts.DTOs.CabinetParts;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetParts.Queries;

public sealed class GetCabinetPartsQueryHandler : IRequestHandler<GetCabinetPartsQuery, List<CabinetPartDto>>
{
    private readonly ProjectsDbContext _dbContext;

    public GetCabinetPartsQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CabinetPartDto>> Handle(GetCabinetPartsQuery request, CancellationToken cancellationToken)
    {
        var cabinetExists = await _dbContext.Cabinets.AnyAsync(c => c.Id == request.CabinetId, cancellationToken);
        if (!cabinetExists)
            throw new NotFoundException("Cabinet", request.CabinetId);

        var parts = await _dbContext.CabinetParts
            .Where(p => p.CabinetId == request.CabinetId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new CabinetPartDto(
                p.Id,
                p.CabinetId,
                p.SourceAssemblyPartId,
                p.ComponentId,
                p.X,
                p.Y,
                p.Z,
                p.RotationX,
                p.RotationY,
                p.RotationZ,
                p.Shape,
                p.Condition,
                p.Quantity,
                p.QuantityFormula,
                p.SortOrder,
                p.IsEnabled,
                p.MaterialId,
                p.Provides,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return parts;
    }
}
