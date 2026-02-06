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
            request.ComponentId, request.Role, request.Placement,
            request.Length, request.Width,
            request.Quantity, request.QuantityFormula, request.SortOrder,
            request.Cutouts);

        _partRepository.Update(part);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var componentDto = new ComponentDto(
            component.Id, component.Name, component.Tags,
            component.Params, component.IsActive, component.CreatedAt);

        return new AssemblyPartDto(
            part.Id, part.AssemblyId, part.ComponentId,
            part.Role, part.Length, part.Width, part.Placement,
            part.Cutouts,
            part.Quantity, part.QuantityFormula, part.SortOrder,
            componentDto);
    }
}
