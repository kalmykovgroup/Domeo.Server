using System.Security.Claims;
using Clients.Abstractions.DTOs;
using Clients.Abstractions.Queries;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Clients.API.Application.Queries;

public sealed class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetClientByIdQueryHandler(IClientRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ClientDto> Handle(
        GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();

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
