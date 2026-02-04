using Audit.Abstractions.Commands;
using Audit.Abstractions.DTOs;
using Audit.Abstractions.Entities;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Application;
using MediatR;

namespace Audit.API.Application.LoginSessions.Commands;

public sealed class CreateLoginSessionCommandHandler
    : IRequestHandler<CreateLoginSessionCommand, LoginSessionDto>
{
    private readonly ILoginSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLoginSessionCommandHandler(
        ILoginSessionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginSessionDto> Handle(
        CreateLoginSessionCommand request, CancellationToken cancellationToken)
    {
        var session = LoginSession.Create(
            Guid.NewGuid(),
            request.UserId,
            request.UserRole,
            request.IpAddress,
            request.UserAgent);

        _repository.Add(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginSessionDto(
            session.Id,
            session.UserId,
            session.UserRole,
            session.IpAddress,
            session.UserAgent,
            session.LoggedInAt,
            session.LoggedOutAt,
            session.IsActive);
    }
}
