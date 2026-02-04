using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Abstractions.Queries.ModuleTypes;

namespace Modules.API.Controllers;

[ApiController]
[Route("types")]
[Tags("Module Types")]
public class ModuleTypesController : ControllerBase
{
    private readonly ISender _sender;

    public ModuleTypesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetModuleTypes(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? limit)
    {
        var result = await _sender.Send(new GetModuleTypesQuery(categoryId, activeOnly, search, page, limit));

        if (!result.IsSuccess)
            return Ok(ApiResponse<List<ModuleTypeDto>>.Fail(result.Error.Description));

        var response = result.Value;

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

    [HttpGet("count")]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetModuleTypesCount(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search)
    {
        var result = await _sender.Send(new GetModuleTypesCountQuery(categoryId, activeOnly, search));
        return result.IsSuccess
            ? Ok(ApiResponse<object>.Ok(new { count = result.Value }))
            : Ok(ApiResponse<object>.Fail(result.Error.Description));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetModuleType(int id)
    {
        var result = await _sender.Send(new GetModuleTypeByIdQuery(id));
        return result.IsSuccess
            ? Ok(ApiResponse<ModuleTypeDto>.Ok(result.Value))
            : Ok(ApiResponse<ModuleTypeDto>.Fail(result.Error.Description));
    }
}
