using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Abstractions.Commands.Zones;
using Projects.Abstractions.DTOs;
using Projects.Abstractions.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.Zones)]
[Tags("Zones")]
public class ZonesController : ControllerBase
{
    private readonly ISender _sender;

    public ZonesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "Permission:projects:read")]
    public async Task<IActionResult> GetZones(Guid edgeId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetZonesQuery(edgeId), cancellationToken);
        return Ok(ApiResponse<List<ZoneDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> CreateZone(
        Guid edgeId,
        [FromBody] CreateZoneRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateZoneCommand(edgeId, request.Type, request.StartX, request.EndX, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Zone created successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.ZoneById)]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> UpdateZone(
        Guid edgeId,
        Guid zoneId,
        [FromBody] UpdateZoneRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateZoneCommand(edgeId, zoneId, request.Name, request.StartX, request.EndX);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Zone updated successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.ZoneById)]
    [Authorize(Policy = "Permission:projects:delete")]
    public async Task<IActionResult> DeleteZone(Guid edgeId, Guid zoneId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteZoneCommand(edgeId, zoneId), cancellationToken);
        return Ok(ApiResponse.Ok("Zone deleted successfully"));
    }
}
