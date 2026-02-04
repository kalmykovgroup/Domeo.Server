using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Queries.ModuleTypes;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.ModuleTypes.Queries;

public sealed class GetModuleTypeByIdQueryHandler : IQueryHandler<GetModuleTypeByIdQuery, ModuleTypeDto>
{
    private readonly IModuleTypeRepository _repository;

    public GetModuleTypeByIdQueryHandler(IModuleTypeRepository repository)
        => _repository = repository;

    public async Task<Result<ModuleTypeDto>> Handle(
        GetModuleTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var moduleType = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (moduleType is null)
            return Result.Failure<ModuleTypeDto>(Error.NotFound("ModuleType.NotFound", "Module type not found"));

        return Result.Success(ToDto(moduleType));
    }

    private static ModuleTypeDto ToDto(ModuleType m) => new(
        m.Id,
        m.CategoryId,
        m.Type,
        m.Name,
        m.WidthDefault,
        m.WidthMin,
        m.WidthMax,
        m.HeightDefault,
        m.HeightMin,
        m.HeightMax,
        m.DepthDefault,
        m.DepthMin,
        m.DepthMax,
        m.PanelThickness,
        m.BackPanelThickness,
        m.FacadeThickness,
        m.FacadeGap,
        m.FacadeOffset,
        m.ShelfSideGap,
        m.ShelfRearInset,
        m.ShelfFrontInset,
        m.IsActive);
}
