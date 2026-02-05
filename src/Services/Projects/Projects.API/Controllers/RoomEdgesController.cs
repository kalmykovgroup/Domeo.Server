using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Application.Commands.RoomEdges;
using Projects.Contracts.DTOs.RoomEdges;
using Projects.Contracts.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.RoomEdges)]
[Tags("Room Edges")]
public class RoomEdgesController : ControllerBase
{
    private readonly ISender _sender;

    public RoomEdgesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "Permission:projects:read")]
    public async Task<IActionResult> GetRoomEdges(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRoomEdgesQuery(roomId), cancellationToken);
        return Ok(ApiResponse<List<RoomEdgeDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> CreateRoomEdge(
        Guid roomId,
        [FromBody] CreateRoomEdgeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomEdgeCommand(
            roomId,
            request.StartVertexId,
            request.EndVertexId,
            request.WallHeight,
            request.HasWindow,
            request.HasDoor,
            request.OrderIndex);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Edge created successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.EdgeById)]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> UpdateRoomEdge(
        Guid roomId,
        Guid edgeId,
        [FromBody] UpdateRoomEdgeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoomEdgeCommand(
            roomId,
            edgeId,
            request.WallHeight,
            request.HasWindow,
            request.HasDoor,
            request.OrderIndex);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Edge updated successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.EdgeById)]
    [Authorize(Policy = "Permission:projects:delete")]
    public async Task<IActionResult> DeleteRoomEdge(Guid roomId, Guid edgeId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRoomEdgeCommand(roomId, edgeId), cancellationToken);
        return Ok(ApiResponse.Ok("Edge deleted successfully"));
    }
}
