using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.AssemblyParts;
using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Repositories;

namespace Modules.API.Application.AssemblyParts.Commands;

public sealed class UpdateAssemblyPartCommandHandler : IRequestHandler<UpdateAssemblyPartCommand, AssemblyPartDto>
{
    private readonly IAssemblyPartRepository _partRepository;
    private readonly IComponentRepository _componentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAssemblyPartCommandHandler(
        IAssemblyPartRepository partRepository,
        IComponentRepository componentRepository,
        IUnitOfWork unitOfWork)
    {
        _partRepository = partRepository;
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssemblyPartDto> Handle(
        UpdateAssemblyPartCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"AssemblyPart {request.Id} not found");

        var component = await _componentRepository.GetByIdAsync(request.ComponentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Component {request.ComponentId} not found");

        part.Update(
            request.ComponentId, request.Role,
            request.LengthExpr, request.WidthExpr,
            request.X, request.Y, request.Z,
            request.RotationX, request.RotationY, request.RotationZ,
            request.Condition,
            request.Quantity, request.QuantityFormula, request.SortOrder,
            request.Shape);

        _partRepository.Update(part);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var componentDto = new ComponentDto(
            component.Id, component.Name, component.Tags,
            component.Params, component.Color, component.IsActive, component.CreatedAt);

        return new AssemblyPartDto(
            part.Id, part.AssemblyId, part.ComponentId,
            part.Role, part.LengthExpr, part.WidthExpr,
            part.X, part.Y, part.Z,
            part.RotationX, part.RotationY, part.RotationZ,
            part.Condition,
            part.Shape,
            part.Quantity, part.QuantityFormula, part.SortOrder,
            componentDto);
    }
}
