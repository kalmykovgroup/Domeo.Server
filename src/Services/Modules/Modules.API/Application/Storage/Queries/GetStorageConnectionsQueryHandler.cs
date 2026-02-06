using MediatR;
using Modules.Application.Queries.Storage;
using Modules.Contracts.DTOs.Storage;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Storage.Queries;

public sealed class GetStorageConnectionsQueryHandler
    : IRequestHandler<GetStorageConnectionsQuery, List<StorageConnectionDto>>
{
    private readonly IStorageConnectionRepository _repository;

    public GetStorageConnectionsQueryHandler(IStorageConnectionRepository repository)
        => _repository = repository;

    public async Task<List<StorageConnectionDto>> Handle(
        GetStorageConnectionsQuery request, CancellationToken cancellationToken)
    {
        var connections = await _repository.GetAllAsync(cancellationToken);

        return connections.Select(c => new StorageConnectionDto(
            c.Id, c.Name, c.Type, c.Endpoint, c.Bucket,
            c.Region, c.IsActive, c.CreatedAt, c.UpdatedAt)).ToList();
    }
}
