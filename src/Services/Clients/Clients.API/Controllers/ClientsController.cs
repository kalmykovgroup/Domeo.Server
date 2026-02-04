using Clients.Abstractions.Commands;
using Clients.Abstractions.Queries;
using Clients.Abstractions.Routes;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clients.API.Controllers;

[ApiController]
[Route(ClientsRoutes.Controller.Base)]
[Tags("Clients")]
public class ClientsController : ControllerBase
{
    private readonly ISender _sender;

    public ClientsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> GetClients(
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder)
    {
        var result = await _sender.Send(new GetClientsQuery(search, page, pageSize, sortBy, sortOrder));
        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedResponse<ClientDto>>.Ok(result.Value))
            : Ok(ApiResponse<PaginatedResponse<ClientDto>>.Fail(result.Error.Description));
    }

    [HttpGet(ClientsRoutes.Controller.ById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> GetClient(Guid id)
    {
        var result = await _sender.Send(new GetClientByIdQuery(id));
        return result.IsSuccess
            ? Ok(ApiResponse<ClientDto>.Ok(result.Value))
            : Ok(ApiResponse<ClientDto>.Fail(result.Error.Description));
    }

    [HttpPost]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
    {
        var result = await _sender.Send(new CreateClientCommand(
            request.Name, request.Phone, request.Email, request.Address, request.Notes));
        return result.IsSuccess
            ? Ok(ApiResponse<ClientDto>.Ok(result.Value, "Client created successfully"))
            : Ok(ApiResponse<ClientDto>.Fail(result.Error.Description));
    }

    [HttpPut(ClientsRoutes.Controller.ById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest request)
    {
        var result = await _sender.Send(new UpdateClientCommand(
            id, request.Name, request.Phone, request.Email, request.Address, request.Notes));
        return result.IsSuccess
            ? Ok(ApiResponse<ClientDto>.Ok(result.Value, "Client updated successfully"))
            : Ok(ApiResponse<ClientDto>.Fail(result.Error.Description));
    }

    [HttpPost(ClientsRoutes.Controller.Restore)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> RestoreClient(Guid id)
    {
        var result = await _sender.Send(new RestoreClientCommand(id));
        return result.IsSuccess
            ? Ok(ApiResponse<ClientDto>.Ok(result.Value, "Client restored successfully"))
            : Ok(ApiResponse<ClientDto>.Fail(result.Error.Description));
    }

    [HttpDelete(ClientsRoutes.Controller.ById)]
    [Authorize(Roles = "designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        var result = await _sender.Send(new DeleteClientCommand(id));
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Client deleted successfully"))
            : Ok(ApiResponse.Fail(result.Error.Description));
    }
}

public sealed record CreateClientRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes);

public sealed record UpdateClientRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes);
