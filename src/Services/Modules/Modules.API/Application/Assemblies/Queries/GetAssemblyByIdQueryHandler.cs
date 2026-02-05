using Domeo.Shared.Exceptions;
using MediatR;
using Modules.Abstractions.DTOs;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Queries.Assemblies;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Assemblies.Queries;

public sealed class GetAssemblyByIdQueryHandler : IRequestHandler<GetAssemblyByIdQuery, AssemblyDetailDto>
{
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IAssemblyPartRepository _partRepository;
    private readonly IComponentRepository _componentRepository;

    public GetAssemblyByIdQueryHandler(
        IAssemblyRepository assemblyRepository,
        IAssemblyPartRepository partRepository,
        IComponentRepository componentRepository)
    {
        _assemblyRepository = assemblyRepository;
        _partRepository = partRepository;
        _componentRepository = componentRepository;
    }

    public async Task<AssemblyDetailDto> Handle(
        GetAssemblyByIdQuery request, CancellationToken cancellationToken)
    {
        var assembly = await _assemblyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (assembly is null)
            throw new NotFoundException("Assembly", request.Id);

        var parts = await _partRepository.GetByAssemblyIdAsync(request.Id, cancellationToken);

        var componentIds = parts.Select(p => p.ComponentId).Distinct().ToList();
        var components = await _componentRepository.GetByIdsAsync(componentIds, cancellationToken);
        var componentMap = components.ToDictionary(c => c.Id);

        return new AssemblyDetailDto(
            assembly.Id,
            assembly.CategoryId,
            assembly.Type,
            assembly.Name,
            assembly.Dimensions,
            assembly.Constraints,
            assembly.Construction,
            assembly.IsActive,
            assembly.CreatedAt,
            parts.Select(p => ToPartDto(p, componentMap)).ToList());
    }

    private static AssemblyPartDto ToPartDto(AssemblyPart p, Dictionary<Guid, Component> componentMap) => new(
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
            : null);
}
