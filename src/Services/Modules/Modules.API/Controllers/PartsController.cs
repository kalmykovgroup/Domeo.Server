using Domeo.Shared.Contracts;
using Modules.Contracts.DTOs.AssemblyParts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Application.Commands.AssemblyParts;
using Modules.Contracts.Routes;

namespace Modules.API.Controllers;

[ApiController]
[Route(ModulesRoutes.Controller.Parts)]
[Tags("Assembly Parts")]
public class PartsController : ControllerBase
{
    private readonly ISender _sender;

    public PartsController(ISender sender) => _sender = sender;

    [HttpPut(ModulesRoutes.ById)]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<AssemblyPartDto>>> UpdatePart(
        Guid id, [FromBody] UpdateAssemblyPartRequest request)
    {
        var result = await _sender.Send(new UpdateAssemblyPartCommand(
            id,
            request.ComponentId,
            request.LengthExpr,
            request.WidthExpr,
            request.X, request.Y, request.Z,
            request.RotationX, request.RotationY, request.RotationZ,
            request.Condition,
            request.Shape,
            request.Quantity,
            request.QuantityFormula,
            request.SortOrder));

        return Ok(ApiResponse<AssemblyPartDto>.Ok(result));
    }

    [HttpDelete(ModulesRoutes.ById)]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<IActionResult> DeletePart(Guid id)
    {
        await _sender.Send(new DeleteAssemblyPartCommand(id));
        return Ok(ApiResponse.Ok("Part deleted"));
    }
}
