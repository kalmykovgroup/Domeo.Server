using System.Security.Claims;
using Clients.Contracts.DTOs;
using Clients.Application.Queries;
using Clients.Domain.Repositories;
using Domeo.Shared.Contracts;
using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Clients.API.Application.Queries;

public sealed class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, PaginatedResponse<ClientDto>>
{
    private readonly IClientRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetClientsQueryHandler(IClientRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginatedResponse<ClientDto>> Handle(
        GetClientsQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();

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
