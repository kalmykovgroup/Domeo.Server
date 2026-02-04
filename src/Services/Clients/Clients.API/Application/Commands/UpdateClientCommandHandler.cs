using Auth.Contracts;
using Clients.Abstractions.Commands;
using Clients.Abstractions.DTOs;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Clients.API.Application.Commands;

public sealed class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public UpdateClientCommandHandler(
        IClientRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<ClientDto> Handle(
        UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id
            ?? throw new UnauthorizedException();

        var client = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Client", request.Id);

        if (client.UserId != userId)
            throw new ForbiddenException("Access denied to this client");

        client.Update(request.Name, request.Phone, request.Email, request.Address, request.Notes);
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
