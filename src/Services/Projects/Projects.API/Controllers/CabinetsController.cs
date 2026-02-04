using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Abstractions.Commands.Cabinets;
using Projects.Abstractions.DTOs;
using Projects.Abstractions.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.Cabinets)]
[Tags("Cabinets")]
public class CabinetsController : ControllerBase
{
    private readonly ISender _sender;

    public CabinetsController(ISender sender) => _sender = sender;

    [HttpGet(ProjectsRoutes.Controller.CabinetsByRoom)]
    [Authorize(Policy = "Permission:cabinets:read")]
    public async Task<IActionResult> GetCabinetsByRoom(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCabinetsByRoomQuery(roomId), cancellationToken);
        return Ok(ApiResponse<List<CabinetDto>>.Ok(result));
    }

    [HttpGet(ProjectsRoutes.Controller.CabinetById)]
    [Authorize(Policy = "Permission:cabinets:read")]
    public async Task<IActionResult> GetCabinet(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCabinetByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<CabinetDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:cabinets:write")]
    public async Task<IActionResult> CreateCabinet(
        [FromBody] CreateCabinetRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCabinetCommand(
            request.RoomId,
            request.PlacementType,
            request.PositionX,
            request.PositionY,
            request.Width,
            request.Height,
            request.Depth,
            request.Name,
            request.EdgeId,
            request.ZoneId,
            request.ModuleTypeId,
            request.FacadeType,
            request.Rotation);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Cabinet created successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.CabinetById)]
    [Authorize(Policy = "Permission:cabinets:write")]
    public async Task<IActionResult> UpdateCabinet(
        Guid id,
        [FromBody] UpdateCabinetRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCabinetCommand(
            id,
            request.PositionX,
            request.PositionY,
            request.Rotation,
            request.Width,
            request.Height,
            request.Depth,
            request.Name,
            request.EdgeId,
            request.ZoneId,
            request.ModuleTypeId,
            request.FacadeType,
            request.CalculatedPrice);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Cabinet updated successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.CabinetById)]
    [Authorize(Policy = "Permission:cabinets:delete")]
    public async Task<IActionResult> DeleteCabinet(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCabinetCommand(id), cancellationToken);
        return Ok(ApiResponse.Ok("Cabinet deleted successfully"));
    }
}
