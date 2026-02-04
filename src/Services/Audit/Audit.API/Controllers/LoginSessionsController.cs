using Audit.Abstractions.Commands;
using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.LoginSessions;
using Audit.Abstractions.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Audit.API.Controllers;

[ApiController]
[Route(AuditRoutes.Controller.LoginSessions)]
[Tags("Login Sessions")]
public class LoginSessionsController : ControllerBase
{
    private readonly ISender _sender;

    public LoginSessionsController(ISender sender) => _sender = sender;

    /// <summary>
    /// Internal API - called by Auth.API
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "InternalApi")]
    public async Task<ActionResult<LoginSessionDto>> CreateLoginSession([FromBody] CreateLoginSessionRequest request)
    {
        var result = await _sender.Send(new CreateLoginSessionCommand(
            request.UserId,
            request.UserRole,
            request.IpAddress,
            request.UserAgent));

        return Ok(result);
    }

    /// <summary>
    /// Internal API - called by Auth.API
    /// </summary>
    [HttpPut(AuditRoutes.Controller.Logout)]
    [Authorize(Policy = "InternalApi")]
    public async Task<IActionResult> LogoutSession(Guid id)
    {
        await _sender.Send(new LogoutSessionCommand(id));
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<object>> GetLoginSessions(
        [FromQuery] Guid? userId,
        [FromQuery] bool? activeOnly,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetLoginSessionsQuery(
            userId, activeOnly, from, to, page, pageSize));

        return Ok(result);
    }

    [HttpGet(AuditRoutes.Controller.ById)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<LoginSessionDto>> GetLoginSession(Guid id)
    {
        var result = await _sender.Send(new GetLoginSessionByIdQuery(id));
        return Ok(result);
    }

    [HttpGet(AuditRoutes.Controller.UserSessions)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<object>> GetUserSessions(
        Guid userId,
        [FromQuery] bool? activeOnly,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetUserSessionsQuery(
            userId, activeOnly, page, pageSize));

        return Ok(result);
    }
}
