using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Storage;
using Modules.Contracts.DTOs.Storage;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Storage.Commands;

public sealed class UpdateStorageConnectionCommandHandler
    : IRequestHandler<UpdateStorageConnectionCommand, StorageConnectionDto>
{
    private readonly IStorageConnectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStorageConnectionCommandHandler(
        IStorageConnectionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StorageConnectionDto> Handle(
        UpdateStorageConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"StorageConnection {request.Id} not found");

        connection.Update(
            request.Name, request.Endpoint, request.Bucket,
            request.Region, request.AccessKey, request.SecretKey,
            request.IsActive);

        _repository.Update(connection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StorageConnectionDto(
            connection.Id, connection.Name, connection.Type, connection.Endpoint,
            connection.Bucket, connection.Region, connection.IsActive,
            connection.CreatedAt, connection.UpdatedAt);
    }
}
