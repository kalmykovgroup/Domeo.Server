using MediatR;
using Modules.Application.Queries.Assemblies;
using Modules.Contracts.DTOs.Assemblies;
using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Assemblies.Queries;

public sealed class GetAssembliesQueryHandler : IRequestHandler<GetAssembliesQuery, AssembliesResponse>
{
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IAssemblyPartRepository _partRepository;
    private readonly IComponentRepository _componentRepository;

    public GetAssembliesQueryHandler(
        IAssemblyRepository assemblyRepository,
        IAssemblyPartRepository partRepository,
        IComponentRepository componentRepository)
    {
        _assemblyRepository = assemblyRepository;
        _partRepository = partRepository;
        _componentRepository = componentRepository;
    }

    public async Task<AssembliesResponse> Handle(
        GetAssembliesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _assemblyRepository.GetAssembliesAsync(
            request.CategoryId, request.ActiveOnly, request.Search,
            request.Page, request.Limit, cancellationToken);

        var allParts = await _partRepository.GetAllAsync(cancellationToken);
        var partsLookup = allParts.GroupBy(p => p.AssemblyId)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.SortOrder).ToList());

        var componentIds = allParts.Select(p => p.ComponentId).Distinct().ToList();
        var components = await _componentRepository.GetByIdsAsync(componentIds, cancellationToken);
        var componentMap = components.ToDictionary(c => c.Id);

        var dtos = items.Select(a => ToDto(a, partsLookup, componentMap)).ToList();

        return request.Page.HasValue && request.Limit.HasValue
            ? new AssembliesResponse(dtos, total, request.Page, request.Limit)
            : new AssembliesResponse(dtos);
    }

    private static AssemblyDto ToDto(
        Assembly a,
        Dictionary<Guid, List<AssemblyPart>> partsLookup,
        Dictionary<Guid, Component> componentMap)
    {
        var parts = partsLookup.TryGetValue(a.Id, out var list) ? list : [];

        return new AssemblyDto(
            a.Id, a.CategoryId, a.Type, a.Name,
            a.Dimensions, a.Constraints, a.Construction,
            a.IsActive, a.CreatedAt,
            parts.Select(p => ToPartDto(p, componentMap)).ToList());
    }

    private static AssemblyPartDto ToPartDto(AssemblyPart p, Dictionary<Guid, Component> componentMap) => new(
        p.Id, p.AssemblyId, p.ComponentId, p.Role,
        p.Length, p.Width, p.Placement,
        p.Cutouts,
        p.Quantity, p.QuantityFormula, p.SortOrder,
        componentMap.TryGetValue(p.ComponentId, out var c)
            ? new ComponentDto(c.Id, c.Name, c.Tags, c.Params, c.IsActive, c.CreatedAt)
            : null);
}
