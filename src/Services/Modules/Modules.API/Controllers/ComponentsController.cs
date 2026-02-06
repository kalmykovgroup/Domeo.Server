using Domeo.Shared.Contracts;
using Modules.Contracts.DTOs.Components;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Application.Commands.Components;
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

    [HttpGet(ModulesRoutes.ById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> GetComponentById(Guid id)
    {
        var result = await _sender.Send(new GetComponentByIdQuery(id));
        if (result is null)
            return NotFound(ApiResponse.Fail($"Component {id} not found"));
        return Ok(ApiResponse<ComponentDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<ComponentDto>>> CreateComponent(
        [FromBody] CreateComponentRequest request)
    {
        var result = await _sender.Send(new CreateComponentCommand(
            request.Name, request.Params, request.Tags, request.Color));
        return Created($"components/{result.Id}", ApiResponse<ComponentDto>.Ok(result));
    }

    [HttpPut(ModulesRoutes.ById)]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<ComponentDto>>> UpdateComponent(
        Guid id, [FromBody] UpdateComponentRequest request)
    {
        var result = await _sender.Send(new UpdateComponentCommand(
            id, request.Name, request.Params, request.Tags, request.Color));
        return Ok(ApiResponse<ComponentDto>.Ok(result));
    }

    [HttpDelete(ModulesRoutes.ById)]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<IActionResult> DeleteComponent(Guid id)
    {
        await _sender.Send(new DeleteComponentCommand(id));
        return Ok(ApiResponse.Ok("Component deleted"));
    }

    [HttpPost("{id:guid}/glb")]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<ComponentDto>>> UploadGlb(
        Guid id, IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is empty"));

        if (!file.FileName.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse.Fail("Only .glb files are allowed"));

        await using var stream = file.OpenReadStream();
        var result = await _sender.Send(new UploadComponentGlbCommand(id, file.FileName, stream));
        return Ok(ApiResponse<ComponentDto>.Ok(result));
    }
}
