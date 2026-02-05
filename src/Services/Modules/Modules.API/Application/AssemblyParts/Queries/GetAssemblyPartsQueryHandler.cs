using MediatR;
using Modules.Abstractions.DTOs;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Queries.AssemblyParts;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.AssemblyParts.Queries;

public sealed class GetAssemblyPartsQueryHandler : IRequestHandler<GetAssemblyPartsQuery, List<AssemblyPartDto>>
{
    private readonly IAssemblyPartRepository _repository;
    private readonly IComponentRepository _componentRepository;

    public GetAssemblyPartsQueryHandler(
        IAssemblyPartRepository repository,
        IComponentRepository componentRepository)
    {
        _repository = repository;
        _componentRepository = componentRepository;
    }

    public async Task<List<AssemblyPartDto>> Handle(
        GetAssemblyPartsQuery request, CancellationToken cancellationToken)
    {
        var parts = await _repository.GetByAssemblyIdAsync(request.AssemblyId, cancellationToken);

        var componentIds = parts.Select(p => p.ComponentId).Distinct().ToList();
        var components = await _componentRepository.GetByIdsAsync(componentIds, cancellationToken);
        var componentMap = components.ToDictionary(c => c.Id);

        return parts.Select(p => new AssemblyPartDto(
            p.Id,
            p.AssemblyId,
            p.ComponentId,
            p.Role.ToString(),
            p.Length,
            p.Width,
            p.Placement,
            p.Quantity,
            p.QuantityFormula,
            p.SortOrder,
            componentMap.TryGetValue(p.ComponentId, out var c)
                ? new ComponentDto(c.Id, c.Name, c.Tags, c.Params, c.IsActive, c.CreatedAt)
                : null)).ToList();
    }
}
