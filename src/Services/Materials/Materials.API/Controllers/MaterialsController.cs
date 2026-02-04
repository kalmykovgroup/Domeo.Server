using Domeo.Shared.Contracts;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.Queries.Materials;
using Materials.Abstractions.Routes;
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
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaterialsQuery(categoryId, activeOnly), cancellationToken);
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
