using Domeo.Shared.Contracts;
using Modules.Contracts.DTOs.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Application.Queries.Categories;
using Modules.Contracts.Routes;

namespace Modules.API.Controllers;

[ApiController]
[Route(ModulesRoutes.Controller.Categories)]
[Tags("Module Categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<ModuleCategoryDto>>>> GetCategories([FromQuery] bool? activeOnly)
    {
        var result = await _sender.Send(new GetCategoriesQuery(activeOnly));
        return Ok(ApiResponse<List<ModuleCategoryDto>>.Ok(result));
    }
}
