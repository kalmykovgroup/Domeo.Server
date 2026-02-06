using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Components;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Components.Commands;

public sealed class DeleteComponentCommandHandler : IRequestHandler<DeleteComponentCommand>
{
    private readonly IComponentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteComponentCommandHandler(IComponentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteComponentCommand request, CancellationToken cancellationToken)
    {
        var component = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Component {request.Id} not found");

        _repository.Remove(component);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
