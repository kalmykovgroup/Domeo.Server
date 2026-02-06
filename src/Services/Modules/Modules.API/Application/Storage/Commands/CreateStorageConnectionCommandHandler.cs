using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Storage;
using Modules.Contracts.DTOs.Storage;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Storage.Commands;

public sealed class CreateStorageConnectionCommandHandler
    : IRequestHandler<CreateStorageConnectionCommand, StorageConnectionDto>
{
    private readonly IStorageConnectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStorageConnectionCommandHandler(
        IStorageConnectionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StorageConnectionDto> Handle(
        CreateStorageConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = StorageConnection.Create(
            request.Name, request.Endpoint, request.Bucket,
            request.Region, request.AccessKey, request.SecretKey);

        _repository.Add(connection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StorageConnectionDto(
            connection.Id, connection.Name, connection.Type, connection.Endpoint,
            connection.Bucket, connection.Region, connection.IsActive,
            connection.CreatedAt, connection.UpdatedAt);
    }
}
