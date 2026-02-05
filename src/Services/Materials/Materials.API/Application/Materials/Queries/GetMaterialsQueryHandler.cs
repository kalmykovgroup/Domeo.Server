using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Materials;
using MediatR;

namespace Materials.API.Application.Materials.Queries;

public sealed class GetMaterialsQueryHandler : IRequestHandler<GetMaterialsQuery, List<MaterialDto>>
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

    public async Task<List<MaterialDto>> Handle(
        GetMaterialsQuery request,
        CancellationToken cancellationToken)
    {
        var externalMaterials = await _supplierApiClient.GetMaterialsAsync(
            request.CategoryId,
            request.ActiveOnly ?? true,
            request.BrandId,
            request.SupplierId,
            request.Attributes,
            cancellationToken);

        var materials = externalMaterials.Select(m => new MaterialDto(
            m.Id,
            m.CategoryId,
            m.BrandId,
            m.BrandName,
            m.Name,
            m.Description,
            m.Unit,
            m.Color,
            m.TextureUrl,
            m.IsActive)).ToList();

        return materials;
    }
}
