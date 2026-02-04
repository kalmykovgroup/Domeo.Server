using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Materials;

namespace Materials.API.Application.Materials.Queries;

public sealed class GetMaterialByIdQueryHandler : IQueryHandler<GetMaterialByIdQuery, MaterialDto>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetMaterialByIdQueryHandler> _logger;

    public GetMaterialByIdQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetMaterialByIdQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<Result<MaterialDto>> Handle(
        GetMaterialByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var externalMaterial = await _supplierApiClient.GetMaterialAsync(request.Id, cancellationToken);

            if (externalMaterial == null)
                return Result.Failure<MaterialDto>(Error.NotFound("Material not found"));

            var material = new MaterialDto(
                externalMaterial.Id,
                externalMaterial.CategoryId,
                externalMaterial.Name,
                externalMaterial.Description,
                externalMaterial.Unit,
                externalMaterial.Color,
                externalMaterial.TextureUrl,
                externalMaterial.IsActive);

            return Result.Success(material);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Supplier service unavailable");
            return Result.Failure<MaterialDto>(
                Error.ServiceUnavailable($"Supplier service unavailable: {ex.Message}"));
        }
    }
}
