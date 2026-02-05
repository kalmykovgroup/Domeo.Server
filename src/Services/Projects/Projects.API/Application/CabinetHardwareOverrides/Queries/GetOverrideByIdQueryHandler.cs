using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.CabinetHardwareOverrides;
using Projects.Contracts.DTOs.CabinetHardwareOverrides;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetHardwareOverrides.Queries;

public sealed class GetOverrideByIdQueryHandler : IRequestHandler<GetOverrideByIdQuery, CabinetHardwareOverrideDto>
{
    private readonly ProjectsDbContext _dbContext;

    public GetOverrideByIdQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CabinetHardwareOverrideDto> Handle(GetOverrideByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.CabinetHardwareOverrides.FindAsync([request.Id], cancellationToken);

        if (entity is null)
            throw new NotFoundException("Override", request.Id);

        return new CabinetHardwareOverrideDto(
            entity.Id,
            entity.CabinetId,
            entity.AssemblyPartId,
            entity.ComponentId,
            entity.Role,
            entity.QuantityFormula,
            entity.PositionXFormula,
            entity.PositionYFormula,
            entity.PositionZFormula,
            entity.IsEnabled,
            entity.MaterialId,
            entity.CreatedAt,
            entity.UpdatedAt ?? entity.CreatedAt);
    }
}
