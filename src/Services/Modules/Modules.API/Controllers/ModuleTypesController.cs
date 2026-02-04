using Domeo.Shared.Contracts;
using Modules.Abstractions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Abstractions.Queries.ModuleTypes;
using Modules.Abstractions.Routes;

namespace Modules.API.Controllers;

[ApiController]
[Route(ModulesRoutes.Controller.Types)]
[Tags("Module Types")]
public class ModuleTypesController : ControllerBase
{
    private readonly ISender _sender;

    public ModuleTypesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> GetModuleTypes(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? limit)
    {
        var response = await _sender.Send(new GetModuleTypesQuery(categoryId, activeOnly, search, page, limit));

        if (response.IsPaginated)
        {
            return Ok(ApiResponse<PaginatedResponse<ModuleTypeDto>>.Ok(
                new PaginatedResponse<ModuleTypeDto>(
                    response.Total!.Value,
                    response.Page!.Value,
                    response.Limit!.Value,
                    response.Items)));
        }

        return Ok(ApiResponse<List<ModuleTypeDto>>.Ok(response.Items));
    }

    [HttpGet(ModulesRoutes.Controller.Count)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> GetModuleTypesCount(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search)
    {
        var count = await _sender.Send(new GetModuleTypesCountQuery(categoryId, activeOnly, search));
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    [HttpGet(ModulesRoutes.Controller.TypeById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<ModuleTypeDto>>> GetModuleType(int id)
    {
        var result = await _sender.Send(new GetModuleTypeByIdQuery(id));
        return Ok(ApiResponse<ModuleTypeDto>.Ok(result));
    }
}
