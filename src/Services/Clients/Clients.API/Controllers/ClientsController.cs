using Clients.Application.Commands;
using Clients.Contracts.DTOs;
using Clients.Application.Queries;
using Clients.Contracts.Routes;
using Domeo.Shared.Contracts;
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
        return Ok(ApiResponse<PaginatedResponse<ClientDto>>.Ok(result));
    }

    [HttpGet(ClientsRoutes.Controller.ById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> GetClient(Guid id)
    {
        var result = await _sender.Send(new GetClientByIdQuery(id));
        return Ok(ApiResponse<ClientDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
    {
        var result = await _sender.Send(new CreateClientCommand(
            request.Name, request.Phone, request.Email, request.Address, request.Notes));
        return Ok(ApiResponse<ClientDto>.Ok(result, "Client created successfully"));
    }

    [HttpPut(ClientsRoutes.Controller.ById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest request)
    {
        var result = await _sender.Send(new UpdateClientCommand(
            id, request.Name, request.Phone, request.Email, request.Address, request.Notes));
        return Ok(ApiResponse<ClientDto>.Ok(result, "Client updated successfully"));
    }

    [HttpPost(ClientsRoutes.Controller.Restore)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> RestoreClient(Guid id)
    {
        var result = await _sender.Send(new RestoreClientCommand(id));
        return Ok(ApiResponse<ClientDto>.Ok(result, "Client restored successfully"));
    }

    [HttpDelete(ClientsRoutes.Controller.ById)]
    [Authorize(Roles = "designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        await _sender.Send(new DeleteClientCommand(id));
        return Ok(ApiResponse.Ok("Client deleted successfully"));
    }
}
