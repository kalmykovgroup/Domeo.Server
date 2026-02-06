using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Components;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Components.Commands;

public sealed class UpdateComponentCommandHandler : IRequestHandler<UpdateComponentCommand, ComponentDto>
{
    private readonly IComponentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComponentCommandHandler(IComponentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ComponentDto> Handle(
        UpdateComponentCommand request, CancellationToken cancellationToken)
    {
        var component = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Component {request.Id} not found");

        component.Update(request.Name, request.Params, request.Tags);
        _repository.Update(component);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ComponentDto(
            component.Id,
            component.Name,
            component.Tags,
            component.Params,
            component.IsActive,
            component.CreatedAt);
    }
}
