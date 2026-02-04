using Clients.Abstractions.Commands;
using Clients.Abstractions.Repositories;
using Domeo.Shared.Auth;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Clients.API.Application.Commands;

public sealed class DeleteClientCommandHandler : ICommandHandler<DeleteClientCommand>
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

    public async Task<Result> Handle(
        DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id;
        if (userId is null)
            return Result.Failure(Error.Failure("Client.Unauthorized", "Unauthorized"));

        var client = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (client is null)
            return Result.Failure(Error.Failure("Client.NotFound", "Client not found"));

        if (client.UserId != userId)
            return Result.Failure(Error.Failure("Client.AccessDenied", "Access denied"));

        client.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
