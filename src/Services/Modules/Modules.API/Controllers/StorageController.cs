using Domeo.Shared.Contracts;
using Modules.Contracts.DTOs.Storage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Application.Commands.Storage;
using Modules.Application.Queries.Storage;
using Modules.Contracts.Routes;

namespace Modules.API.Controllers;

[ApiController]
[Route(ModulesRoutes.Controller.Storage)]
[Tags("Storage")]
public class StorageController : ControllerBase
{
    private readonly ISender _sender;

    public StorageController(ISender sender) => _sender = sender;

    [HttpGet("connections")]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<StorageConnectionDto>>>> GetConnections()
    {
        var result = await _sender.Send(new GetStorageConnectionsQuery());
        return Ok(ApiResponse<List<StorageConnectionDto>>.Ok(result));
    }

    [HttpPost("connections")]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<ApiResponse<StorageConnectionDto>>> CreateConnection(
        [FromBody] CreateStorageConnectionRequest request)
    {
        var result = await _sender.Send(new CreateStorageConnectionCommand(
            request.Name, request.Endpoint, request.Bucket,
            request.Region, request.AccessKey, request.SecretKey));

        return Created($"storage/connections/{result.Id}", ApiResponse<StorageConnectionDto>.Ok(result));
    }

    [HttpPut("connections/{id:guid}")]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<ApiResponse<StorageConnectionDto>>> UpdateConnection(
        Guid id, [FromBody] UpdateStorageConnectionRequest request)
    {
        var result = await _sender.Send(new UpdateStorageConnectionCommand(
            id, request.Name, request.Endpoint, request.Bucket,
            request.Region, request.AccessKey, request.SecretKey,
            request.IsActive));

        return Ok(ApiResponse<StorageConnectionDto>.Ok(result));
    }

    [HttpDelete("connections/{id:guid}")]
    [Authorize(Roles = "systemAdmin")]
    public async Task<IActionResult> DeleteConnection(Guid id)
    {
        await _sender.Send(new DeleteStorageConnectionCommand(id));
        return Ok(ApiResponse.Ok("Storage connection deleted"));
    }

    [HttpPost("connections/{id:guid}/test")]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> TestConnection(Guid id)
    {
        var result = await _sender.Send(new TestStorageConnectionCommand(id));
        return Ok(ApiResponse<bool>.Ok(result, result ? "Connection successful" : "Connection failed"));
    }

    [HttpPost("migrate")]
    [Authorize(Roles = "systemAdmin")]
    public async Task<ActionResult<ApiResponse<int>>> Migrate()
    {
        var count = await _sender.Send(new MigrateToRemoteStorageCommand());
        return Ok(ApiResponse<int>.Ok(count, $"Migrated {count} files"));
    }
}
