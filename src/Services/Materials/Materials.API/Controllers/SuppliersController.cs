using Domeo.Shared.Contracts;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.Queries.Suppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Controllers;

[ApiController]
[Route("suppliers")]
[Tags("Suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public SuppliersController(ISender sender) => _sender = sender;

    /// <summary>
    /// Get all suppliers
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Permission:suppliers:read")]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSuppliersQuery(activeOnly), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<List<SupplierDto>>.Ok(result.Value))
            : Ok(ApiResponse<List<SupplierDto>>.Fail(result.Error.Description));
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:suppliers:read")]
    public async Task<IActionResult> GetSupplier(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<SupplierDto>.Ok(result.Value))
            : Ok(ApiResponse<SupplierDto>.Fail(result.Error.Description));
    }
}
