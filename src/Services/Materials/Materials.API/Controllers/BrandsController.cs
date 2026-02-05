using Domeo.Shared.Contracts;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.Queries.Brands;
using Materials.Abstractions.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Controllers;

[ApiController]
[Route(MaterialsRoutes.Controller.Brands)]
[Tags("Brands")]
public class BrandsController : ControllerBase
{
    private readonly ISender _sender;

    public BrandsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<BrandDto>>>> GetBrands(
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBrandsQuery(activeOnly), cancellationToken);
        return Ok(ApiResponse<List<BrandDto>>.Ok(result));
    }
}
