using Audit.Contracts.DTOs.ApplicationLogs;
using Audit.Application.Queries.ApplicationLogs;
using Audit.Domain.Repositories;
using Domeo.Shared.Exceptions;
using MediatR;

namespace Audit.API.Application.ApplicationLogs.Queries;

public sealed class GetApplicationLogByIdQueryHandler
    : IRequestHandler<GetApplicationLogByIdQuery, ApplicationLogDto>
{
    private readonly IApplicationLogRepository _repository;

    public GetApplicationLogByIdQueryHandler(IApplicationLogRepository repository)
        => _repository = repository;

    public async Task<ApplicationLogDto> Handle(
        GetApplicationLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (log is null)
            throw new NotFoundException("ApplicationLog", request.Id);

        return new ApplicationLogDto(
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
    }
}
