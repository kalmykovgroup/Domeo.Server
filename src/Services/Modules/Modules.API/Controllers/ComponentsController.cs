using Domeo.Shared.Contracts;
using Modules.Contracts.DTOs.Components;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Application.Queries.Components;
using Modules.Contracts.Routes;

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
}
