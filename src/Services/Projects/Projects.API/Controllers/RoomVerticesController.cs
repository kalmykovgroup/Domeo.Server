using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Application.Commands.RoomVertices;
using Projects.Contracts.DTOs.RoomVertices;
using Projects.Contracts.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.RoomVertices)]
[Tags("Room Vertices")]
public class RoomVerticesController : ControllerBase
{
    private readonly ISender _sender;

    public RoomVerticesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "Permission:projects:read")]
    public async Task<IActionResult> GetRoomVertices(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRoomVerticesQuery(roomId), cancellationToken);
        return Ok(ApiResponse<List<RoomVertexDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> CreateRoomVertex(
        Guid roomId,
        [FromBody] CreateRoomVertexRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomVertexCommand(roomId, request.X, request.Y, request.OrderIndex);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Vertex created successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.VertexById)]
    [Authorize(Policy = "Permission:projects:delete")]
    public async Task<IActionResult> DeleteRoomVertex(Guid roomId, Guid vertexId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRoomVertexCommand(roomId, vertexId), cancellationToken);
        return Ok(ApiResponse.Ok("Vertex deleted successfully"));
    }
}
