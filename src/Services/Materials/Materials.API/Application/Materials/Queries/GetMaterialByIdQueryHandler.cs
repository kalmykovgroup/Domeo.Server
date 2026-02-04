using Domeo.Shared.Exceptions;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Materials;
using MediatR;

namespace Materials.API.Application.Materials.Queries;

public sealed class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, MaterialDto>
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

    public async Task<MaterialDto> Handle(
        GetMaterialByIdQuery request,
        CancellationToken cancellationToken)
    {
        var externalMaterial = await _supplierApiClient.GetMaterialAsync(request.Id, cancellationToken);

        if (externalMaterial == null)
            throw new NotFoundException("Material", request.Id);

        var material = new MaterialDto(
            externalMaterial.Id,
            externalMaterial.CategoryId,
            externalMaterial.Name,
            externalMaterial.Description,
            externalMaterial.Unit,
            externalMaterial.Color,
            externalMaterial.TextureUrl,
            externalMaterial.IsActive);

        return material;
    }
}
