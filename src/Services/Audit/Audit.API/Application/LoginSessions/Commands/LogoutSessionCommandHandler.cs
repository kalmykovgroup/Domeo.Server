using Audit.Abstractions.Commands;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Audit.API.Application.LoginSessions.Commands;

public sealed class LogoutSessionCommandHandler
    : ICommandHandler<LogoutSessionCommand>
{
    private readonly ILoginSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutSessionCommandHandler(
        ILoginSessionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        LogoutSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            return Result.Failure(Error.Failure("LoginSession.NotFound", "Session not found"));

        if (!session.IsActive)
            return Result.Failure(Error.Failure("LoginSession.AlreadyLoggedOut", "Session already logged out"));

        session.Logout();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
