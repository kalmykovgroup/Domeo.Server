using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.ApplicationLogs;
using Audit.Abstractions.Routes;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Audit.API.Controllers;

[ApiController]
[Route(AuditRoutes.Controller.ApplicationLogs)]
[Tags("Application Logs")]
public class ApplicationLogsController : ControllerBase
{
    private readonly ISender _sender;

    public ApplicationLogsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> GetApplicationLogs(
        [FromQuery] string? level,
        [FromQuery] string? serviceName,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetApplicationLogsQuery(
            level, serviceName, userId, from, to, page, pageSize));

        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedResponse<ApplicationLogDto>>.Ok(result.Value))
            : Ok(ApiResponse<PaginatedResponse<ApplicationLogDto>>.Fail(result.Error.Description));
    }

    [HttpGet(AuditRoutes.Controller.ById)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> GetApplicationLog(Guid id)
    {
        var result = await _sender.Send(new GetApplicationLogByIdQuery(id));

        return result.IsSuccess
            ? Ok(ApiResponse<ApplicationLogDto>.Ok(result.Value))
            : Ok(ApiResponse<ApplicationLogDto>.Fail(result.Error.Description));
    }

    [HttpGet(AuditRoutes.Controller.Stats)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> GetLogStats(
        [FromQuery] string? groupBy,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _sender.Send(new GetApplicationLogStatsQuery(groupBy, from, to));

        return result.IsSuccess
            ? Ok(ApiResponse<object>.Ok(result.Value))
            : Ok(ApiResponse<object>.Fail(result.Error.Description));
    }
}
