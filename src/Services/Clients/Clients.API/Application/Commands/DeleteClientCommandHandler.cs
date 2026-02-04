using Auth.Contracts;
using Clients.Abstractions.Commands;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Clients.API.Application.Commands;

public sealed class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand>
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public DeleteClientCommandHandler(
        IClientRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task Handle(
        DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id
            ?? throw new UnauthorizedException();

        var client = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Client", request.Id);

        if (client.UserId != userId)
            throw new ForbiddenException("Access denied to this client");

        client.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
