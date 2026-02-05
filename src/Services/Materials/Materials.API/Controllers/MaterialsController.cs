using Domeo.Shared.Contracts;
using Materials.Contracts.DTOs;
using Materials.Application.Queries.Materials;
using Materials.Application.Queries.Search;
using Materials.Contracts.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Controllers;

[ApiController]
[Route(MaterialsRoutes.Controller.Items)]
[Tags("Materials")]
public class MaterialsController : ControllerBase
{
    private readonly ISender _sender;

    public MaterialsController(ISender sender) => _sender = sender;

    /// <summary>
    /// Get all materials
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<MaterialDto>>>> GetMaterials(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? brandId,
        [FromQuery] string? supplierId,
        CancellationToken cancellationToken)
    {
        // Collect attribute filters from query: attr_{name}={value}
        Dictionary<string, string>? attributes = null;
        foreach (var param in HttpContext.Request.Query)
        {
            if (param.Key.StartsWith("attr_", StringComparison.OrdinalIgnoreCase))
            {
                attributes ??= new Dictionary<string, string>();
                attributes[param.Key[5..]] = param.Value.ToString();
            }
        }

        var result = await _sender.Send(
            new GetMaterialsQuery(categoryId, activeOnly, brandId, supplierId, attributes),
            cancellationToken);
        return Ok(ApiResponse<List<MaterialDto>>.Ok(result));
    }

    /// <summary>
    /// Get material by ID
    /// </summary>
    [HttpGet(MaterialsRoutes.Controller.ItemById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<MaterialDto>>> GetMaterial(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaterialByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<MaterialDto>.Ok(result));
    }

    /// <summary>
    /// Search suggestions across materials, brands, categories and attributes
    /// </summary>
    [HttpGet(MaterialsRoutes.Controller.Suggest)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<SearchSuggestionDto>>>> GetSearchSuggestions(
        [FromQuery] string query,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(ApiResponse<List<SearchSuggestionDto>>.Ok([]));

        var result = await _sender.Send(
            new GetSearchSuggestionsQuery(query, limit ?? 10),
            cancellationToken);
        return Ok(ApiResponse<List<SearchSuggestionDto>>.Ok(result));
    }

    /// <summary>
    /// Get material offers
    /// </summary>
    [HttpGet(MaterialsRoutes.Controller.ItemOffers)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<MaterialOffersDto>>> GetMaterialOffers(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaterialOffersQuery(id), cancellationToken);
        return Ok(ApiResponse<MaterialOffersDto>.Ok(result));
    }
}
