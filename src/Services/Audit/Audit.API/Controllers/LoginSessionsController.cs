using Audit.Abstractions.Commands;
using Audit.Abstractions.DTOs;
using Audit.Abstractions.Queries.LoginSessions;
using Audit.Abstractions.Routes;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
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
    public async Task<IActionResult> CreateLoginSession([FromBody] CreateLoginSessionRequest request)
    {
        var result = await _sender.Send(new CreateLoginSessionCommand(
            request.UserId,
            request.UserEmail,
            request.UserName,
            request.UserRole,
            request.IpAddress,
            request.UserAgent));

        return result.IsSuccess
            ? Ok(ApiResponse<LoginSessionDto>.Ok(result.Value, "Login session created"))
            : Ok(ApiResponse<LoginSessionDto>.Fail(result.Error.Description));
    }

    /// <summary>
    /// Internal API - called by Auth.API
    /// </summary>
    [HttpPut(AuditRoutes.Controller.Logout)]
    [Authorize(Policy = "InternalApi")]
    public async Task<IActionResult> LogoutSession(Guid id)
    {
        var result = await _sender.Send(new LogoutSessionCommand(id));

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Logout recorded"))
            : Ok(ApiResponse.Fail(result.Error.Description));
    }

    [HttpGet]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> GetLoginSessions(
        [FromQuery] Guid? userId,
        [FromQuery] bool? activeOnly,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetLoginSessionsQuery(
            userId, activeOnly, from, to, page, pageSize));

        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedResponse<LoginSessionDto>>.Ok(result.Value))
            : Ok(ApiResponse<PaginatedResponse<LoginSessionDto>>.Fail(result.Error.Description));
    }

    [HttpGet(AuditRoutes.Controller.ById)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> GetLoginSession(Guid id)
    {
        var result = await _sender.Send(new GetLoginSessionByIdQuery(id));

        return result.IsSuccess
            ? Ok(ApiResponse<LoginSessionDto>.Ok(result.Value))
            : Ok(ApiResponse<LoginSessionDto>.Fail(result.Error.Description));
    }

    [HttpGet(AuditRoutes.Controller.UserSessions)]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> GetUserSessions(
        Guid userId,
        [FromQuery] bool? activeOnly,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _sender.Send(new GetUserSessionsQuery(
            userId, activeOnly, page, pageSize));

        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedResponse<LoginSessionDto>>.Ok(result.Value))
            : Ok(ApiResponse<PaginatedResponse<LoginSessionDto>>.Fail(result.Error.Description));
    }
}

public sealed record CreateLoginSessionRequest(
    Guid UserId,
    string UserEmail,
    string? UserName,
    string UserRole,
    string? IpAddress,
    string? UserAgent);
