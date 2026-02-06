using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Assemblies;
using Modules.Contracts.DTOs.Assemblies;
using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Assemblies.Commands;

public sealed class UpdateAssemblyCommandHandler : IRequestHandler<UpdateAssemblyCommand, AssemblyDto>
{
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IAssemblyPartRepository _partRepository;
    private readonly IComponentRepository _componentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAssemblyCommandHandler(
        IAssemblyRepository assemblyRepository,
        IAssemblyPartRepository partRepository,
        IComponentRepository componentRepository,
        IUnitOfWork unitOfWork)
    {
        _assemblyRepository = assemblyRepository;
        _partRepository = partRepository;
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssemblyDto> Handle(
        UpdateAssemblyCommand request, CancellationToken cancellationToken)
    {
        var assembly = await _assemblyRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Assembly {request.Id} not found");

        assembly.Update(request.Name, request.Parameters, request.ParamConstraints);

        _assemblyRepository.Update(assembly);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var allParts = await _partRepository.GetAllAsync(cancellationToken);
        var parts = allParts.Where(p => p.AssemblyId == assembly.Id).OrderBy(p => p.SortOrder).ToList();

        var componentIds = parts.Select(p => p.ComponentId).Distinct().ToList();
        var components = await _componentRepository.GetByIdsAsync(componentIds, cancellationToken);
        var componentMap = components.ToDictionary(c => c.Id);

        return new AssemblyDto(
            assembly.Id, assembly.CategoryId, assembly.Type, assembly.Name,
            assembly.Parameters, assembly.ParamConstraints,
            assembly.IsActive, assembly.CreatedAt,
            parts.Select(p => ToPartDto(p, componentMap)).ToList());
    }

    private static AssemblyPartDto ToPartDto(AssemblyPart p, Dictionary<Guid, Component> componentMap) => new(
        p.Id, p.AssemblyId, p.ComponentId,
        p.X, p.Y, p.Z,
        p.RotationX, p.RotationY, p.RotationZ,
        p.Condition,
        p.Shape,
        p.Quantity, p.QuantityFormula, p.SortOrder,
        p.Provides,
        componentMap.TryGetValue(p.ComponentId, out var c)
            ? new ComponentDto(c.Id, c.Name, c.Tags, c.Params, c.Color, c.IsActive, c.CreatedAt)
            : null);
}
