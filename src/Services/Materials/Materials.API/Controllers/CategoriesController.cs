using Domeo.Shared.Contracts;
using Materials.Contracts.DTOs;
using Materials.Application.Queries.Categories;
using Materials.Contracts.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Controllers;

[ApiController]
[Route(MaterialsRoutes.Controller.Categories)]
[Tags("Categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Get categories tree with supplier IDs
    /// </summary>
    [HttpGet(MaterialsRoutes.Controller.Tree)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<CategoryTreeNodeDto>>>> GetCategoriesTree(
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCategoriesTreeQuery(activeOnly), cancellationToken);
        return Ok(ApiResponse<List<CategoryTreeNodeDto>>.Ok(result));
    }

    /// <summary>
    /// Get attributes for a category
    /// </summary>
    [HttpGet(MaterialsRoutes.Controller.CategoryAttributes)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<CategoryAttributeDto>>>> GetCategoryAttributes(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCategoryAttributesQuery(id), cancellationToken);
        return Ok(ApiResponse<List<CategoryAttributeDto>>.Ok(result));
    }
}
