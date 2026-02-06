using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Storage;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Storage.Commands;

public sealed class DeleteStorageConnectionCommandHandler : IRequestHandler<DeleteStorageConnectionCommand>
{
    private readonly IStorageConnectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStorageConnectionCommandHandler(
        IStorageConnectionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteStorageConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"StorageConnection {request.Id} not found");

        _repository.Remove(connection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
