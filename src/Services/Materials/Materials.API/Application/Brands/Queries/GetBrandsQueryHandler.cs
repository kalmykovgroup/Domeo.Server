using Materials.Contracts.DTOs;
using Materials.Application.ExternalServices;
using Materials.Application.Queries.Brands;
using MediatR;

namespace Materials.API.Application.Brands.Queries;

public sealed class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetBrandsQueryHandler> _logger;

    public GetBrandsQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetBrandsQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<List<BrandDto>> Handle(
        GetBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var externalBrands = await _supplierApiClient.GetBrandsAsync(
            request.ActiveOnly ?? true, cancellationToken);

        return externalBrands.Select(b => new BrandDto(
            b.Id,
            b.Name,
            b.LogoUrl,
            b.IsActive)).ToList();
    }
}
