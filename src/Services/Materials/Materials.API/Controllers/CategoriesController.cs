using Domeo.Shared.Contracts;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Controllers;

[ApiController]
[Route("categories")]
[Tags("Categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Get categories tree with supplier IDs
    /// </summary>
    [HttpGet("tree")]
    [Authorize(Policy = "Permission:catalog:read")]
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
