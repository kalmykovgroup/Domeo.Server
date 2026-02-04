using Clients.Abstractions.Commands;
using Clients.Abstractions.Entities;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Auth;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Clients.API.Application.Commands;

public sealed class CreateClientCommandHandler : ICommandHandler<CreateClientCommand, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateClientCommandHandler(
        IClientRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<ClientDto>> Handle(
        CreateClientCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id;
        if (userId is null)
            return Result.Failure<ClientDto>(Error.Failure("Client.Unauthorized", "Unauthorized"));

        var client = Client.Create(
            request.Name,
            userId.Value,
            request.Phone,
            request.Email,
            request.Address,
            request.Notes);

        _repository.Add(client);
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
