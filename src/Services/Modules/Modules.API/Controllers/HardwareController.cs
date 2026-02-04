using Domeo.Shared.Contracts;
using Modules.Abstractions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Abstractions.Queries.Hardware;
using Modules.Abstractions.Routes;

namespace Modules.API.Controllers;

[ApiController]
[Route(ModulesRoutes.Controller.Hardware)]
[Tags("Hardware")]
public class HardwareController : ControllerBase
{
    private readonly ISender _sender;

    public HardwareController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<HardwareDto>>>> GetHardware(
        [FromQuery] string? type,
        [FromQuery] bool? activeOnly)
    {
        var result = await _sender.Send(new GetHardwareQuery(type, activeOnly));
        return Ok(ApiResponse<List<HardwareDto>>.Ok(result));
    }

    [HttpGet(ModulesRoutes.Controller.HardwareById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<HardwareDto>>> GetHardwareItem(int id)
    {
        var result = await _sender.Send(new GetHardwareByIdQuery(id));
        return Ok(ApiResponse<HardwareDto>.Ok(result));
    }
}
