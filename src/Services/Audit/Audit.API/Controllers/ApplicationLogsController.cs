using Audit.Contracts.DTOs.ApplicationLogs;
using Audit.Application.Queries.ApplicationLogs;
using Audit.Contracts.Routes;
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
    public async Task<ActionResult<object>> GetApplicationLogs(
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

        return Ok(result);
    }

    [HttpGet(AuditRoutes.Controller.ById)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<ApplicationLogDto>> GetApplicationLog(Guid id)
    {
        var result = await _sender.Send(new GetApplicationLogByIdQuery(id));
        return Ok(result);
    }

    [HttpGet(AuditRoutes.Controller.Stats)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<object>> GetLogStats(
        [FromQuery] string? groupBy,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _sender.Send(new GetApplicationLogStatsQuery(groupBy, from, to));
        return Ok(result);
    }
}
