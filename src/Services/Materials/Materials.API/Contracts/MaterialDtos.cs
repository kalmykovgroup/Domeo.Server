namespace Materials.API.Contracts;

// Category tree node for response
public sealed class CategoryTreeNodeDto
{
    public string Id { get; init; } = string.Empty;
    public string? ParentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Level { get; init; }
    public int OrderIndex { get; init; }
    public bool IsActive { get; init; }
    public List<string> SupplierIds { get; init; } = [];
    public List<CategoryTreeNodeDto> Children { get; init; } = [];
}

// Supplier for response
public sealed record SupplierResponseDto(
    string Id,
    string Company,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? Website,
    double Rating,
    bool IsActive);

// Material for response
public sealed record MaterialResponseDto(
    string Id,
    string CategoryId,
    string Name,
    string? Description,
    string Unit,
    string? Color,
    string? TextureUrl,
    bool IsActive);

// Supplier info in offer
public sealed record OfferSupplierDto(
    string Id,
    string Company,
    string? ContactName,
    string? Phone,
    string? Email,
    double Rating);

// Offer for response
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

// Material offers response
public sealed class MaterialOffersResponseDto
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
