using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Abstractions.Queries.Categories;

namespace Modules.API.Controllers;

[ApiController]
[Route("categories")]
[Tags("Module Categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetCategories([FromQuery] bool? activeOnly)
    {
        var result = await _sender.Send(new GetCategoriesQuery(activeOnly));
        return result.IsSuccess
            ? Ok(ApiResponse<List<ModuleCategoryDto>>.Ok(result.Value))
            : Ok(ApiResponse<List<ModuleCategoryDto>>.Fail(result.Error.Description));
    }

    [HttpGet("tree")]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetCategoriesTree([FromQuery] bool? activeOnly)
    {
        var result = await _sender.Send(new GetCategoriesTreeQuery(activeOnly));
        return result.IsSuccess
            ? Ok(ApiResponse<List<ModuleCategoryTreeDto>>.Ok(result.Value))
            : Ok(ApiResponse<List<ModuleCategoryTreeDto>>.Fail(result.Error.Description));
    }
}
