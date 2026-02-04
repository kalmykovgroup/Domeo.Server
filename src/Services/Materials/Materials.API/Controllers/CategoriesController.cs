using Domeo.Shared.Contracts;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.Queries.Categories;
using Materials.Abstractions.Routes;
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
    public async Task<IActionResult> GetCategoriesTree(
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCategoriesTreeQuery(activeOnly), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<List<CategoryTreeNodeDto>>.Ok(result.Value))
            : Ok(ApiResponse<List<CategoryTreeNodeDto>>.Fail(result.Error.Description));
    }
}
