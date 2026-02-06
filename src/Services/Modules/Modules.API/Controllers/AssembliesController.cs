using Domeo.Shared.Contracts;
using Modules.Contracts.DTOs.Assemblies;
using Modules.Contracts.DTOs.AssemblyParts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Application.Commands.Assemblies;
using Modules.Application.Commands.AssemblyParts;
using Modules.Application.Queries.Assemblies;
using Modules.Contracts.Routes;

namespace Modules.API.Controllers;

[ApiController]
[Route(ModulesRoutes.Controller.Assemblies)]
[Tags("Assemblies")]
public class AssembliesController : ControllerBase
{
    private readonly ISender _sender;

    public AssembliesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> GetAssemblies(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? limit)
    {
        var response = await _sender.Send(new GetAssembliesQuery(categoryId, activeOnly, search, page, limit));

        if (response.IsPaginated)
        {
            return Ok(ApiResponse<PaginatedResponse<AssemblyDto>>.Ok(
                new PaginatedResponse<AssemblyDto>(
                    response.Total!.Value,
                    response.Page!.Value,
                    response.Limit!.Value,
                    response.Items)));
        }

        return Ok(ApiResponse<List<AssemblyDto>>.Ok(response.Items));
    }

    [HttpPost]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<AssemblyDto>>> CreateAssembly(
        [FromBody] CreateAssemblyRequest request)
    {
        var result = await _sender.Send(new CreateAssemblyCommand(
            request.CategoryId,
            request.Type,
            request.Name,
            request.Parameters,
            request.ParamConstraints));

        return Created($"assemblies/{result.Id}", ApiResponse<AssemblyDto>.Ok(result));
    }

    [HttpPut(ModulesRoutes.ById)]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<AssemblyDto>>> UpdateAssembly(
        Guid id, [FromBody] UpdateAssemblyRequest request)
    {
        var result = await _sender.Send(new UpdateAssemblyCommand(
            id,
            request.Name,
            request.Parameters,
            request.ParamConstraints));

        return Ok(ApiResponse<AssemblyDto>.Ok(result));
    }

    [HttpDelete(ModulesRoutes.ById)]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<IActionResult> DeleteAssembly(Guid id)
    {
        await _sender.Send(new DeleteAssemblyCommand(id));
        return Ok(ApiResponse.Ok("Assembly deleted"));
    }

    [HttpPost("{assemblyId:guid}/parts")]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<AssemblyPartDto>>> CreatePart(
        Guid assemblyId, [FromBody] CreateAssemblyPartRequest request)
    {
        var result = await _sender.Send(new CreateAssemblyPartCommand(
            assemblyId,
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

        return Created($"parts/{result.Id}", ApiResponse<AssemblyPartDto>.Ok(result));
    }
}
