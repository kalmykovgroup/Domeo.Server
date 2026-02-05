using System.Security.Claims;
using Clients.Application.Commands;
using Clients.Contracts.DTOs;
using Clients.Domain.Entities;
using Clients.Domain.Repositories;
using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Clients.API.Application.Commands;

public sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateClientCommandHandler(
        IClientRepository repository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ClientDto> Handle(
        CreateClientCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();

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
