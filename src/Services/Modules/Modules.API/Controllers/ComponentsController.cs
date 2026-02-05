using Domeo.Shared.Contracts;
using Modules.Abstractions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Abstractions.Queries.Components;
using Modules.Abstractions.Routes;

namespace Modules.API.Controllers;

[ApiController]
[Route(ModulesRoutes.Controller.Components)]
[Tags("Components")]
public class ComponentsController : ControllerBase
{
    private readonly ISender _sender;

    public ComponentsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<ComponentDto>>>> GetComponents(
        [FromQuery] string? tag,
        [FromQuery] bool? activeOnly)
    {
        var result = await _sender.Send(new GetComponentsQuery(tag, activeOnly));
        return Ok(ApiResponse<List<ComponentDto>>.Ok(result));
    }

    [HttpGet(ModulesRoutes.Controller.ComponentById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<ComponentDto>>> GetComponent(Guid id)
    {
        var result = await _sender.Send(new GetComponentByIdQuery(id));
        return Ok(ApiResponse<ComponentDto>.Ok(result));
    }
}
