using Domeo.Shared.Contracts;
using Modules.Contracts.DTOs.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Application.Commands.Categories;
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

    [HttpPost]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<ModuleCategoryDto>>> CreateCategory(
        [FromBody] CreateCategoryRequest request)
    {
        var result = await _sender.Send(new CreateCategoryCommand(
            request.Id, request.Name, request.ParentId, request.Description, request.OrderIndex));

        return Created($"categories/{result.Id}", ApiResponse<ModuleCategoryDto>.Ok(result));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<ModuleCategoryDto>>> UpdateCategory(
        string id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _sender.Send(new UpdateCategoryCommand(
            id, request.Name, request.Description, request.OrderIndex));

        return Ok(ApiResponse<ModuleCategoryDto>.Ok(result));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        await _sender.Send(new DeleteCategoryCommand(id));
        return Ok(ApiResponse.Ok("Category deleted"));
    }
}
