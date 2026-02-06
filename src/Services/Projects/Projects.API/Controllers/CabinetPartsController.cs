using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Application.Commands.CabinetParts;
using Projects.Application.Queries.CabinetParts;
using Projects.Contracts.DTOs.CabinetParts;
using Projects.Contracts.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.CabinetParts)]
[Tags("Cabinet Parts")]
public class CabinetPartsController : ControllerBase
{
    private readonly ISender _sender;

    public CabinetPartsController(ISender sender) => _sender = sender;

    [HttpGet(ProjectsRoutes.Controller.CabinetPartsByCabinet)]
    [Authorize(Policy = "Permission:cabinets:read")]
    public async Task<IActionResult> GetPartsByCabinet(Guid cabinetId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCabinetPartsQuery(cabinetId), cancellationToken);
        return Ok(ApiResponse<List<CabinetPartDto>>.Ok(result));
    }

    [HttpPost(ProjectsRoutes.Controller.CabinetPartsByCabinet)]
    [Authorize(Policy = "Permission:cabinets:write")]
    public async Task<IActionResult> CreatePart(
        Guid cabinetId,
        [FromBody] CreateCabinetPartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCabinetPartCommand(
            cabinetId,
            request.ComponentId,
            request.SourceAssemblyPartId,
            request.X,
            request.Y,
            request.Z,
            request.RotationX,
            request.RotationY,
            request.RotationZ,
            request.Shape,
            request.Condition,
            request.Quantity,
            request.QuantityFormula,
            request.SortOrder,
            request.IsEnabled,
            request.MaterialId,
            request.Provides);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Cabinet part created successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.CabinetPartById)]
    [Authorize(Policy = "Permission:cabinets:write")]
    public async Task<IActionResult> UpdatePart(
        Guid id,
        [FromBody] UpdateCabinetPartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCabinetPartCommand(
            id,
            request.ComponentId,
            request.X,
            request.Y,
            request.Z,
            request.RotationX,
            request.RotationY,
            request.RotationZ,
            request.Shape,
            request.Condition,
            request.Quantity,
            request.QuantityFormula,
            request.SortOrder,
            request.IsEnabled,
            request.MaterialId,
            request.Provides);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Cabinet part updated successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.CabinetPartById)]
    [Authorize(Policy = "Permission:cabinets:delete")]
    public async Task<IActionResult> DeletePart(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCabinetPartCommand(id), cancellationToken);
        return Ok(ApiResponse.Ok("Cabinet part deleted successfully"));
    }
}
