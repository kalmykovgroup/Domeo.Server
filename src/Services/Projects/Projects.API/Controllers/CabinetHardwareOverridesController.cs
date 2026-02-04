using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Abstractions.Commands.CabinetHardwareOverrides;
using Projects.Abstractions.DTOs;
using Projects.Abstractions.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.CabinetHardwareOverrides)]
[Tags("Cabinet Hardware Overrides")]
public class CabinetHardwareOverridesController : ControllerBase
{
    private readonly ISender _sender;

    public CabinetHardwareOverridesController(ISender sender) => _sender = sender;

    [HttpGet(ProjectsRoutes.Controller.HardwareOverridesByCabinet)]
    [Authorize(Policy = "Permission:cabinets:read")]
    public async Task<IActionResult> GetOverridesByCabinet(Guid cabinetId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOverridesByCabinetQuery(cabinetId), cancellationToken);
        return Ok(ApiResponse<List<CabinetHardwareOverrideDto>>.Ok(result));
    }

    [HttpGet(ProjectsRoutes.Controller.HardwareOverrideById)]
    [Authorize(Policy = "Permission:cabinets:read")]
    public async Task<IActionResult> GetOverride(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOverrideByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<CabinetHardwareOverrideDto>.Ok(result));
    }

    [HttpPost(ProjectsRoutes.Controller.HardwareOverridesByCabinet)]
    [Authorize(Policy = "Permission:cabinets:write")]
    public async Task<IActionResult> CreateOverride(
        Guid cabinetId,
        [FromBody] CreateCabinetHardwareOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOverrideCommand(
            cabinetId,
            request.ModuleHardwareId,
            request.IsEnabled,
            request.HardwareId,
            request.Role,
            request.QuantityFormula,
            request.PositionXFormula,
            request.PositionYFormula,
            request.PositionZFormula,
            request.MaterialId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Hardware override created successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.HardwareOverrideById)]
    [Authorize(Policy = "Permission:cabinets:write")]
    public async Task<IActionResult> UpdateOverride(
        Guid id,
        [FromBody] UpdateCabinetHardwareOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOverrideCommand(
            id,
            request.IsEnabled,
            request.HardwareId,
            request.Role,
            request.QuantityFormula,
            request.PositionXFormula,
            request.PositionYFormula,
            request.PositionZFormula,
            request.MaterialId);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Hardware override updated successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.HardwareOverrideById)]
    [Authorize(Policy = "Permission:cabinets:delete")]
    public async Task<IActionResult> DeleteOverride(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteOverrideCommand(id), cancellationToken);
        return Ok(ApiResponse.Ok("Hardware override deleted successfully"));
    }
}
