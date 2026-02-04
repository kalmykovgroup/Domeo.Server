using Auth.Contracts;
using Clients.Abstractions.DTOs;
using Clients.Abstractions.Queries;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Clients.API.Application.Queries;

public sealed class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetClientByIdQueryHandler(IClientRepository repository, ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<ClientDto> Handle(
        GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id
            ?? throw new UnauthorizedException();

        var client = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Client", request.Id);

        if (client.UserId != userId)
            throw new ForbiddenException("Access denied to this client");

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
