using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.AuditLogs;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.API.Application.AuditLogs.Queries;

public sealed class GetEntityHistoryQueryHandler
    : IRequestHandler<GetEntityHistoryQuery, PaginatedResponse<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;

    public GetEntityHistoryQueryHandler(IAuditLogRepository repository)
        => _repository = repository;

    public async Task<PaginatedResponse<AuditLogDto>> Handle(
        GetEntityHistoryQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var (items, total) = await _repository.GetEntityHistoryAsync(
            request.EntityType,
            request.EntityId,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(l => new AuditLogDto(
            l.Id,
            l.UserId,
            l.Action,
            l.EntityType,
            l.EntityId,
            l.ServiceName,
            l.OldValue,
            l.NewValue,
            l.IpAddress,
            l.CreatedAt)).ToList();

        return new PaginatedResponse<AuditLogDto>(total, page, pageSize, dtos);
    }
}
