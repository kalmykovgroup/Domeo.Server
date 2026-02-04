using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.AuditLogs;
using Audit.Abstractions.Routes;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
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
    public async Task<IActionResult> GetAuditLogs(
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

        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedResponse<AuditLogDto>>.Ok(result.Value))
            : Ok(ApiResponse<PaginatedResponse<AuditLogDto>>.Fail(result.Error.Description));
    }

    [HttpGet(AuditRoutes.Controller.EntityHistory)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> GetEntityHistory(
        string entityType,
        string entityId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetEntityHistoryQuery(
            entityType, entityId, page, pageSize));

        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedResponse<AuditLogDto>>.Ok(result.Value))
            : Ok(ApiResponse<PaginatedResponse<AuditLogDto>>.Fail(result.Error.Description));
    }
}
