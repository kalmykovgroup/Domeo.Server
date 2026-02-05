using Audit.Contracts.DTOs.LoginSessions;
using Audit.Application.Queries.LoginSessions;
using Audit.Domain.Repositories;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.API.Application.LoginSessions.Queries;

public sealed class GetLoginSessionsQueryHandler
    : IRequestHandler<GetLoginSessionsQuery, PaginatedResponse<LoginSessionDto>>
{
    private readonly ILoginSessionRepository _repository;

    public GetLoginSessionsQueryHandler(ILoginSessionRepository repository)
        => _repository = repository;

    public async Task<PaginatedResponse<LoginSessionDto>> Handle(
        GetLoginSessionsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var (items, total) = await _repository.GetSessionsAsync(
            request.UserId,
            request.ActiveOnly,
            request.From,
            request.To,
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
