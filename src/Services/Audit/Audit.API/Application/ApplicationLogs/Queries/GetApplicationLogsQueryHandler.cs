using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.ApplicationLogs;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Audit.API.Application.ApplicationLogs.Queries;

public sealed class GetApplicationLogsQueryHandler
    : IQueryHandler<GetApplicationLogsQuery, PaginatedResponse<ApplicationLogDto>>
{
    private readonly IApplicationLogRepository _repository;

    public GetApplicationLogsQueryHandler(IApplicationLogRepository repository)
        => _repository = repository;

    public async Task<Result<PaginatedResponse<ApplicationLogDto>>> Handle(
        GetApplicationLogsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var (items, total) = await _repository.GetLogsAsync(
            request.Level,
            request.ServiceName,
            request.UserId,
            request.From,
            request.To,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(l => new ApplicationLogDto(
            l.Id,
            l.ServiceName,
            l.Level,
            l.Message,
            l.Exception,
            l.ExceptionType,
            l.Properties,
            l.RequestPath,
            l.UserId,
            l.CorrelationId,
            l.CreatedAt)).ToList();

        return Result.Success(new PaginatedResponse<ApplicationLogDto>(total, page, pageSize, dtos));
    }
}
