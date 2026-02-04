using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.ApplicationLogs;
using Audit.Abstractions.Repositories;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;

namespace Audit.API.Application.ApplicationLogs.Queries;

public sealed class GetApplicationLogByIdQueryHandler
    : IQueryHandler<GetApplicationLogByIdQuery, ApplicationLogDto>
{
    private readonly IApplicationLogRepository _repository;

    public GetApplicationLogByIdQueryHandler(IApplicationLogRepository repository)
        => _repository = repository;

    public async Task<Result<ApplicationLogDto>> Handle(
        GetApplicationLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (log is null)
            return Result.Failure<ApplicationLogDto>(
                Error.Failure("ApplicationLog.NotFound", "Application log not found"));

        var dto = new ApplicationLogDto(
            log.Id,
            log.ServiceName,
            log.Level,
            log.Message,
            log.Exception,
            log.ExceptionType,
            log.Properties,
            log.RequestPath,
            log.UserId,
            log.CorrelationId,
            log.CreatedAt);

        return Result.Success(dto);
    }
}
