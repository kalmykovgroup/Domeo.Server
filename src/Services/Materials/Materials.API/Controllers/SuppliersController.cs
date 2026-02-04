using Domeo.Shared.Contracts;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.Queries.Suppliers;
using Materials.Abstractions.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Controllers;

[ApiController]
[Route(MaterialsRoutes.Controller.Suppliers)]
[Tags("Suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public SuppliersController(ISender sender) => _sender = sender;

    /// <summary>
    /// Get all suppliers
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> GetSuppliers(
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSuppliersQuery(activeOnly), cancellationToken);
        return Ok(ApiResponse<List<SupplierDto>>.Ok(result));
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    [HttpGet(MaterialsRoutes.Controller.SupplierById)]
    [Authorize(Roles = "catalogAdmin,systemAdmin")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<SupplierDto>.Ok(result));
    }
}
