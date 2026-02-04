namespace Materials.Abstractions.DTOs;

public sealed class MaterialOffersDto
{
    public MaterialBriefDto Material { get; init; } = null!;
    public List<OfferDto> Offers { get; init; } = [];
    public int TotalOffers { get; init; }
}

public sealed record MaterialBriefDto(
    string Id,
    string Name,
    string Unit,
    string? Description);

public sealed record OfferDto(
    string OfferId,
    string MaterialId,
    decimal Price,
    string Currency,
    int MinOrderQty,
    int LeadTimeDays,
    bool InStock,
    string? Sku,
    string? Notes,
    DateTime UpdatedAt,
    OfferSupplierDto Supplier);

public sealed record OfferSupplierDto(
    string Id,
    string Company,
    string? ContactName,
    string? Phone,
    string? Email,
    double Rating);
