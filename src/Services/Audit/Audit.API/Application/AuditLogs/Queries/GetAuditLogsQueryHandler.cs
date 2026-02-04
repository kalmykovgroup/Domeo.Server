using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.AuditLogs;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Audit.API.Application.AuditLogs.Queries;

public sealed class GetAuditLogsQueryHandler
    : IQueryHandler<GetAuditLogsQuery, PaginatedResponse<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;

    public GetAuditLogsQueryHandler(IAuditLogRepository repository)
        => _repository = repository;

    public async Task<Result<PaginatedResponse<AuditLogDto>>> Handle(
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
            l.UserEmail,
            l.Action,
            l.EntityType,
            l.EntityId,
            l.ServiceName,
            l.OldValue,
            l.NewValue,
            l.IpAddress,
            l.CreatedAt)).ToList();

        return Result.Success(new PaginatedResponse<AuditLogDto>(total, page, pageSize, dtos));
    }
}
