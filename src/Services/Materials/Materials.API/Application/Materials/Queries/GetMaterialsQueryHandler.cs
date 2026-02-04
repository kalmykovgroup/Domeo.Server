using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Materials;

namespace Materials.API.Application.Materials.Queries;

public sealed class GetMaterialsQueryHandler : IQueryHandler<GetMaterialsQuery, List<MaterialDto>>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetMaterialsQueryHandler> _logger;

    public GetMaterialsQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetMaterialsQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<Result<List<MaterialDto>>> Handle(
        GetMaterialsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var externalMaterials = await _supplierApiClient.GetMaterialsAsync(
                request.CategoryId, request.ActiveOnly ?? true, cancellationToken);

            var materials = externalMaterials.Select(m => new MaterialDto(
                m.Id,
                m.CategoryId,
                m.Name,
                m.Description,
                m.Unit,
                m.Color,
                m.TextureUrl,
                m.IsActive)).ToList();

            return Result.Success(materials);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Supplier service unavailable");
            return Result.Failure<List<MaterialDto>>(
                Error.ServiceUnavailable($"Supplier service unavailable: {ex.Message}"));
        }
    }
}
