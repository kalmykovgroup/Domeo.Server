using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Materials;

namespace Materials.API.Application.Materials.Queries;

public sealed class GetMaterialOffersQueryHandler : IQueryHandler<GetMaterialOffersQuery, MaterialOffersDto>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetMaterialOffersQueryHandler> _logger;

    public GetMaterialOffersQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetMaterialOffersQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<Result<MaterialOffersDto>> Handle(
        GetMaterialOffersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var externalOffers = await _supplierApiClient.GetOffersAsync(request.MaterialId, cancellationToken);

            if (externalOffers == null)
                return Result.Failure<MaterialOffersDto>(Error.NotFound("Material not found"));

            var response = new MaterialOffersDto
            {
                Material = new MaterialBriefDto(
                    externalOffers.Material.Id,
                    externalOffers.Material.Name,
                    externalOffers.Material.Unit,
                    externalOffers.Material.Description),
                Offers = externalOffers.Offers.Select(o => new OfferDto(
                    o.OfferId,
                    o.MaterialId,
                    o.Price,
                    o.Currency,
                    o.MinOrderQty,
                    o.LeadTimeDays,
                    o.InStock,
                    o.Sku,
                    o.Notes,
                    o.UpdatedAt,
                    new OfferSupplierDto(
                        o.Supplier.Id,
                        o.Supplier.Company,
                        o.Supplier.ContactName,
                        o.Supplier.Phone,
                        o.Supplier.Email,
                        o.Supplier.Rating))).ToList(),
                TotalOffers = externalOffers.TotalOffers
            };

            return Result.Success(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Supplier service unavailable");
            return Result.Failure<MaterialOffersDto>(
                Error.ServiceUnavailable($"Supplier service unavailable: {ex.Message}"));
        }
    }
}
