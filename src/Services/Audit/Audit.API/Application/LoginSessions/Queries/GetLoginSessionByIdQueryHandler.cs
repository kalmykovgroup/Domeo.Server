using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.LoginSessions;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Audit.API.Application.LoginSessions.Queries;

public sealed class GetLoginSessionByIdQueryHandler
    : IQueryHandler<GetLoginSessionByIdQuery, LoginSessionDto>
{
    private readonly ILoginSessionRepository _repository;

    public GetLoginSessionByIdQueryHandler(ILoginSessionRepository repository)
        => _repository = repository;

    public async Task<Result<LoginSessionDto>> Handle(
        GetLoginSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (session is null)
            return Result.Failure<LoginSessionDto>(
                Error.Failure("LoginSession.NotFound", "Session not found"));

        var dto = new LoginSessionDto(
            session.Id,
            session.UserId,
            session.UserEmail,
            session.UserName,
            session.UserRole,
            session.IpAddress,
            session.UserAgent,
            session.LoggedInAt,
            session.LoggedOutAt,
            session.IsActive);

        return Result.Success(dto);
    }
}
