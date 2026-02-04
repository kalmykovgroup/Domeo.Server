using Audit.Abstractions.Commands;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Audit.API.Application.LoginSessions.Commands;

public sealed class LogoutSessionCommandHandler
    : IRequestHandler<LogoutSessionCommand>
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

    public async Task Handle(
        LogoutSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            throw new NotFoundException("LoginSession", request.SessionId);

        if (!session.IsActive)
            throw new ConflictException("Session already logged out");

        session.Logout();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
