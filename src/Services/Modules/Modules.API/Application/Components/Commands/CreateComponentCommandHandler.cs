using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Components;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Components.Commands;

public sealed class CreateComponentCommandHandler : IRequestHandler<CreateComponentCommand, ComponentDto>
{
    private readonly IComponentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateComponentCommandHandler(IComponentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ComponentDto> Handle(
        CreateComponentCommand request, CancellationToken cancellationToken)
    {
        var component = Component.Create(request.Name, request.Params, request.Tags, request.Color);
        _repository.Add(component);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ComponentDto(
            component.Id,
            component.Name,
            component.Tags,
            component.Params,
            component.Color,
            component.IsActive,
            component.CreatedAt);
    }
}
