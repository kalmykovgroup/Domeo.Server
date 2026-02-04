using Clients.Abstractions.Queries;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Auth;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Clients.API.Application.Queries;

public sealed class GetClientsQueryHandler : IQueryHandler<GetClientsQuery, PaginatedResponse<ClientDto>>
{
    private readonly IClientRepository _repository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetClientsQueryHandler(IClientRepository repository, ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<PaginatedResponse<ClientDto>>> Handle(
        GetClientsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id;
        if (userId is null)
            return Result.Failure<PaginatedResponse<ClientDto>>(
                Error.Failure("Client.Unauthorized", "Unauthorized"));

        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var (items, total) = await _repository.GetClientsAsync(
            userId.Value,
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

        return Result.Success(new PaginatedResponse<ClientDto>(total, page, pageSize, dtos));
    }
}
