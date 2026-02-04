using Domeo.Shared.Exceptions;
using MediatR;
using Modules.Abstractions.DTOs;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Queries.ModuleTypes;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.ModuleTypes.Queries;

public sealed class GetModuleTypeByIdQueryHandler : IRequestHandler<GetModuleTypeByIdQuery, ModuleTypeDto>
{
    private readonly IModuleTypeRepository _repository;

    public GetModuleTypeByIdQueryHandler(IModuleTypeRepository repository)
        => _repository = repository;

    public async Task<ModuleTypeDto> Handle(
        GetModuleTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var moduleType = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (moduleType is null)
            throw new NotFoundException("ModuleType", request.Id);

        return ToDto(moduleType);
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
