using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Abstractions.Queries.Hardware;

namespace Modules.API.Controllers;

[ApiController]
[Route("hardware")]
[Tags("Hardware")]
public class HardwareController : ControllerBase
{
    private readonly ISender _sender;

    public HardwareController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetHardware(
        [FromQuery] string? type,
        [FromQuery] bool? activeOnly)
    {
        var result = await _sender.Send(new GetHardwareQuery(type, activeOnly));
        return result.IsSuccess
            ? Ok(ApiResponse<List<HardwareDto>>.Ok(result.Value))
            : Ok(ApiResponse<List<HardwareDto>>.Fail(result.Error.Description));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetHardwareItem(int id)
    {
        var result = await _sender.Send(new GetHardwareByIdQuery(id));
        return result.IsSuccess
            ? Ok(ApiResponse<HardwareDto>.Ok(result.Value))
            : Ok(ApiResponse<HardwareDto>.Fail(result.Error.Description));
    }
}
