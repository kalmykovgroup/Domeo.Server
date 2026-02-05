using Audit.Contracts.DTOs.AuditLogs;
using Audit.Application.Queries.AuditLogs;
using Audit.Domain.Repositories;
using Domeo.Shared.Contracts;
using MediatR;

namespace Audit.API.Application.AuditLogs.Queries;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, PaginatedResponse<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;

    public GetAuditLogsQueryHandler(IAuditLogRepository repository)
        => _repository = repository;

    public async Task<PaginatedResponse<AuditLogDto>> Handle(
        GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var (items, total) = await _repository.GetLogsAsync(
            request.UserId,
            request.Action,
            request.EntityType,
            request.ServiceName,
            request.From,
            request.To,
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
