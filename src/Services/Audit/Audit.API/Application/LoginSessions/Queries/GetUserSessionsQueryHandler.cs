using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.LoginSessions;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.API.Application.LoginSessions.Queries;

public sealed class GetUserSessionsQueryHandler
    : IRequestHandler<GetUserSessionsQuery, PaginatedResponse<LoginSessionDto>>
{
    private readonly ILoginSessionRepository _repository;

    public GetUserSessionsQueryHandler(ILoginSessionRepository repository)
        => _repository = repository;

    public async Task<PaginatedResponse<LoginSessionDto>> Handle(
        GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var (items, total) = await _repository.GetUserSessionsAsync(
            request.UserId,
            request.ActiveOnly,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(s => new LoginSessionDto(
            s.Id,
            s.UserId,
            s.UserRole,
            s.IpAddress,
            s.UserAgent,
            s.LoggedInAt,
            s.LoggedOutAt,
            s.IsActive)).ToList();

        return new PaginatedResponse<LoginSessionDto>(total, page, pageSize, dtos);
    }
}
