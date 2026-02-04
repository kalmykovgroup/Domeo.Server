using Clients.Abstractions.Commands;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Auth;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Clients.API.Application.Commands;

public sealed class RestoreClientCommandHandler : ICommandHandler<RestoreClientCommand, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RestoreClientCommandHandler(
        IClientRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<ClientDto>> Handle(
        RestoreClientCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id;
        if (userId is null)
            return Result.Failure<ClientDto>(Error.Failure("Client.Unauthorized", "Unauthorized"));

        var client = await _repository.GetByIdIncludingDeletedAsync(request.Id, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(Error.Failure("Client.NotFound", "Client not found"));

        if (client.UserId != userId)
            return Result.Failure<ClientDto>(Error.Failure("Client.AccessDenied", "Access denied"));

        if (!client.IsDeleted)
            return Result.Failure<ClientDto>(Error.Failure("Client.NotDeleted", "Client is not deleted"));

        client.Restore();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
