using Clients.Abstractions.Queries;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Auth;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Clients.API.Application.Queries;

public sealed class GetClientByIdQueryHandler : IQueryHandler<GetClientByIdQuery, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetClientByIdQueryHandler(IClientRepository repository, ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<ClientDto>> Handle(
        GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id;
        if (userId is null)
            return Result.Failure<ClientDto>(Error.Failure("Client.Unauthorized", "Unauthorized"));

        var client = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(Error.Failure("Client.NotFound", "Client not found"));

        if (client.UserId != userId)
            return Result.Failure<ClientDto>(Error.Failure("Client.AccessDenied", "Access denied"));

        return Result.Success(new ClientDto(
            client.Id,
            client.Name,
            client.Phone,
            client.Email,
            client.Address,
            client.Notes,
            client.UserId,
            client.CreatedAt,
            client.DeletedAt));
    }
}
