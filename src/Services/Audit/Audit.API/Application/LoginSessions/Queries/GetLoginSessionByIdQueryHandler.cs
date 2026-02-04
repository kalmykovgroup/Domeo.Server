using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.LoginSessions;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Audit.API.Application.LoginSessions.Queries;

public sealed class GetLoginSessionByIdQueryHandler
    : IRequestHandler<GetLoginSessionByIdQuery, LoginSessionDto>
{
    private readonly ILoginSessionRepository _repository;

    public GetLoginSessionByIdQueryHandler(ILoginSessionRepository repository)
        => _repository = repository;

    public async Task<LoginSessionDto> Handle(
        GetLoginSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (session is null)
            throw new NotFoundException("LoginSession", request.Id);

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
