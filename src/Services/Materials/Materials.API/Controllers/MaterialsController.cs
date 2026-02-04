using Domeo.Shared.Contracts;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.Queries.Materials;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Controllers;

[ApiController]
[Route("items")]
[Tags("Materials")]
public class MaterialsController : ControllerBase
{
    private readonly ISender _sender;

    public MaterialsController(ISender sender) => _sender = sender;

    /// <summary>
    /// Get all materials
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetMaterials(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaterialsQuery(categoryId, activeOnly), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<List<MaterialDto>>.Ok(result.Value))
            : Ok(ApiResponse<List<MaterialDto>>.Fail(result.Error.Description));
    }

    /// <summary>
    /// Get material by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetMaterial(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaterialByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<MaterialDto>.Ok(result.Value))
            : Ok(ApiResponse<MaterialDto>.Fail(result.Error.Description));
    }

    /// <summary>
    /// Get material offers
    /// </summary>
    [HttpGet("{id}/offers")]
    [Authorize(Policy = "Permission:catalog:read")]
    public async Task<IActionResult> GetMaterialOffers(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaterialOffersQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<MaterialOffersDto>.Ok(result.Value))
            : Ok(ApiResponse<MaterialOffersDto>.Fail(result.Error.Description));
    }
}
