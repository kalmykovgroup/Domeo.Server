using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Queries.ModuleTypes;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.ModuleTypes.Queries;

public sealed class GetModuleTypesQueryHandler : IQueryHandler<GetModuleTypesQuery, ModuleTypesResponse>
{
    private readonly IModuleTypeRepository _repository;

    public GetModuleTypesQueryHandler(IModuleTypeRepository repository)
        => _repository = repository;

    public async Task<Result<ModuleTypesResponse>> Handle(
        GetModuleTypesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repository.GetModuleTypesAsync(
            request.CategoryId, request.ActiveOnly, request.Search,
            request.Page, request.Limit, cancellationToken);

        var dtos = items.Select(ToDto).ToList();

        var response = request.Page.HasValue && request.Limit.HasValue
            ? new ModuleTypesResponse(dtos, total, request.Page, request.Limit)
            : new ModuleTypesResponse(dtos);

        return Result.Success(response);
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
