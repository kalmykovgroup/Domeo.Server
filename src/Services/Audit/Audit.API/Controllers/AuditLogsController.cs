using Audit.Application.Queries.AuditLogs;
using Audit.Contracts.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Audit.API.Controllers;

[ApiController]
[Route(AuditRoutes.Controller.Logs)]
[Tags("Audit Logs")]
public class AuditLogsController : ControllerBase
{
    private readonly ISender _sender;

    public AuditLogsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<object>> GetAuditLogs(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] string? serviceName,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetAuditLogsQuery(
            userId, action, entityType, serviceName, from, to, page, pageSize));

        return Ok(result);
    }

    [HttpGet(AuditRoutes.Controller.EntityHistory)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<object>> GetEntityHistory(
        string entityType,
        string entityId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetEntityHistoryQuery(
            entityType, entityId, page, pageSize));

        return Ok(result);
    }
}
