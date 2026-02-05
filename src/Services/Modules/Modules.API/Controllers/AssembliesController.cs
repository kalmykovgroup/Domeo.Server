using Domeo.Shared.Contracts;
using Modules.Abstractions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Abstractions.Queries.Assemblies;
using Modules.Abstractions.Queries.AssemblyParts;
using Modules.Abstractions.Routes;

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

    [HttpGet(ModulesRoutes.Controller.Count)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> GetAssembliesCount(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search)
    {
        var count = await _sender.Send(new GetAssembliesCountQuery(categoryId, activeOnly, search));
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    [HttpGet(ModulesRoutes.Controller.AssemblyById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<AssemblyDetailDto>>> GetAssembly(Guid id)
    {
        var result = await _sender.Send(new GetAssemblyByIdQuery(id));
        return Ok(ApiResponse<AssemblyDetailDto>.Ok(result));
    }

    [HttpGet(ModulesRoutes.Controller.AssemblyParts)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<AssemblyPartDto>>>> GetAssemblyParts(Guid id)
    {
        var result = await _sender.Send(new GetAssemblyPartsQuery(id));
        return Ok(ApiResponse<List<AssemblyPartDto>>.Ok(result));
    }
}
