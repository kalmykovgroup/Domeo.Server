using Auth.Contracts;
using Clients.Abstractions.DTOs;
using Clients.Abstractions.Queries;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Contracts;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Clients.API.Application.Queries;

public sealed class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, PaginatedResponse<ClientDto>>
{
    private readonly IClientRepository _repository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetClientsQueryHandler(IClientRepository repository, ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<PaginatedResponse<ClientDto>> Handle(
        GetClientsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id
            ?? throw new UnauthorizedException();

        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var (items, total) = await _repository.GetClientsAsync(
            userId,
            request.Search,
            request.SortBy,
            request.SortOrder,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Phone,
            c.Email,
            c.Address,
            c.Notes,
            c.UserId,
            c.CreatedAt,
            c.DeletedAt)).ToList();

        return new PaginatedResponse<ClientDto>(total, page, pageSize, dtos);
    }
}
