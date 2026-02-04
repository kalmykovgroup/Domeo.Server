using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Abstractions.Commands.Rooms;
using Projects.Abstractions.DTOs;
using Projects.Abstractions.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.Rooms)]
[Tags("Rooms")]
public class RoomsController : ControllerBase
{
    private readonly ISender _sender;

    public RoomsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "Permission:projects:read")]
    public async Task<IActionResult> GetRooms(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRoomsQuery(projectId), cancellationToken);
        return Ok(ApiResponse<List<RoomDto>>.Ok(result));
    }

    [HttpGet(ProjectsRoutes.Controller.RoomById)]
    [Authorize(Policy = "Permission:projects:read")]
    public async Task<IActionResult> GetRoom(Guid projectId, Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRoomByIdQuery(projectId, roomId), cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> CreateRoom(
        Guid projectId,
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomCommand(projectId, request.Name, request.CeilingHeight, request.OrderIndex);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Room created successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.RoomById)]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> UpdateRoom(
        Guid projectId,
        Guid roomId,
        [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoomCommand(projectId, roomId, request.Name, request.CeilingHeight, request.OrderIndex);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Room updated successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.RoomById)]
    [Authorize(Policy = "Permission:projects:delete")]
    public async Task<IActionResult> DeleteRoom(Guid projectId, Guid roomId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRoomCommand(projectId, roomId), cancellationToken);
        return Ok(ApiResponse.Ok("Room deleted successfully"));
    }
}
