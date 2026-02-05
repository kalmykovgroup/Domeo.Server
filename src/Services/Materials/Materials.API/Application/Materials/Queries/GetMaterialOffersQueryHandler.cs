using Domeo.Shared.Exceptions;
using Materials.Contracts.DTOs;
using Materials.Application.ExternalServices;
using Materials.Application.Queries.Materials;
using MediatR;

namespace Materials.API.Application.Materials.Queries;

public sealed class GetMaterialOffersQueryHandler : IRequestHandler<GetMaterialOffersQuery, MaterialOffersDto>
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

    public async Task<MaterialOffersDto> Handle(
        GetMaterialOffersQuery request,
        CancellationToken cancellationToken)
    {
        var externalOffers = await _supplierApiClient.GetOffersAsync(request.MaterialId, cancellationToken);

        if (externalOffers == null)
            throw new NotFoundException("Material", request.MaterialId);

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

        return response;
    }
}
