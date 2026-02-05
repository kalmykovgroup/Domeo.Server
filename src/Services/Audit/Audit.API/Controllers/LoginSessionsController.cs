using Audit.Application.Commands;
using Audit.Contracts.DTOs.LoginSessions;
using Audit.Application.Queries.LoginSessions;
using Audit.Contracts.Routes;
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
