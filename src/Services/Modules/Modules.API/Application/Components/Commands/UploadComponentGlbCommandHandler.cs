using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Components;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities.Shared;
using Modules.Domain.Repositories;
using Modules.Infrastructure.Storage;

namespace Modules.API.Application.Components.Commands;

public sealed class UploadComponentGlbCommandHandler : IRequestHandler<UploadComponentGlbCommand, ComponentDto>
{
    private readonly IComponentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FileStorageResolver _storageResolver;

    public UploadComponentGlbCommandHandler(
        IComponentRepository repository,
        IUnitOfWork unitOfWork,
        FileStorageResolver storageResolver)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _storageResolver = storageResolver;
    }

    public async Task<ComponentDto> Handle(
        UploadComponentGlbCommand request, CancellationToken cancellationToken)
    {
        var component = await _repository.GetByIdAsync(request.ComponentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Component {request.ComponentId} not found");

        var storage = await _storageResolver.ResolveAsync(cancellationToken);
        var fileName = $"{component.Id}_{request.FileName}";
        var url = await storage.UploadAsync(fileName, request.FileStream, cancellationToken);

        var currentParams = component.Params;
        var scale = currentParams is GlbParams glb ? glb.Scale : 1.0;
        component.Update(component.Name, new GlbParams(url, scale), component.Tags);
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
