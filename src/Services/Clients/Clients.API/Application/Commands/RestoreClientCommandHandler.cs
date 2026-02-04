using System.Security.Claims;
using Clients.Abstractions.Commands;
using Clients.Abstractions.DTOs;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Clients.API.Application.Commands;

public sealed class RestoreClientCommandHandler : IRequestHandler<RestoreClientCommand, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RestoreClientCommandHandler(
        IClientRepository repository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ClientDto> Handle(
        RestoreClientCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();

        var client = await _repository.GetByIdIncludingDeletedAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Client", request.Id);

        if (client.UserId != userId)
            throw new ForbiddenException("Access denied to this client");

        if (!client.IsDeleted)
            throw new ConflictException("Client", "Client is not deleted");

        client.Restore();
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
