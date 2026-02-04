using Auth.Contracts;
using Clients.Abstractions.Commands;
using Clients.Abstractions.DTOs;
using Clients.Abstractions.Entities;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Clients.API.Application.Commands;

public sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
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

    public async Task<ClientDto> Handle(
        CreateClientCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id
            ?? throw new UnauthorizedException();

        var client = Client.Create(
            request.Name,
            userId,
            request.Phone,
            request.Email,
            request.Address,
            request.Notes);

        _repository.Add(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientDto(
            client.Id,
            client.Name,
            client.Phone,
            client.Email,
            client.Address,
            client.Notes,
            client.UserId,
            client.CreatedAt,
            client.DeletedAt);
    }
}
