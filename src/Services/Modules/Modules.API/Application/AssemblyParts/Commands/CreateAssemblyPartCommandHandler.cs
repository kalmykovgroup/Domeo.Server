using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.AssemblyParts;
using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.API.Application.AssemblyParts.Commands;

public sealed class CreateAssemblyPartCommandHandler : IRequestHandler<CreateAssemblyPartCommand, AssemblyPartDto>
{
    private readonly IAssemblyPartRepository _partRepository;
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IComponentRepository _componentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAssemblyPartCommandHandler(
        IAssemblyPartRepository partRepository,
        IAssemblyRepository assemblyRepository,
        IComponentRepository componentRepository,
        IUnitOfWork unitOfWork)
    {
        _partRepository = partRepository;
        _assemblyRepository = assemblyRepository;
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssemblyPartDto> Handle(
        CreateAssemblyPartCommand request, CancellationToken cancellationToken)
    {
        _ = await _assemblyRepository.GetByIdAsync(request.AssemblyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Assembly {request.AssemblyId} not found");

        var component = await _componentRepository.GetByIdAsync(request.ComponentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Component {request.ComponentId} not found");

        var part = AssemblyPart.Create(
            request.AssemblyId,
            request.ComponentId,
            request.LengthExpr,
            request.WidthExpr,
            request.X, request.Y, request.Z,
            request.RotationX, request.RotationY, request.RotationZ,
            request.Condition,
            request.Quantity,
            request.QuantityFormula,
            request.SortOrder,
            request.Shape);

        _partRepository.Add(part);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var componentDto = new ComponentDto(
            component.Id, component.Name, component.Tags,
            component.Params, component.Color, component.IsActive, component.CreatedAt);

        return new AssemblyPartDto(
            part.Id, part.AssemblyId, part.ComponentId,
            part.LengthExpr, part.WidthExpr,
            part.X, part.Y, part.Z,
            part.RotationX, part.RotationY, part.RotationZ,
            part.Condition,
            part.Shape,
            part.Quantity, part.QuantityFormula, part.SortOrder,
            componentDto);
    }
}
