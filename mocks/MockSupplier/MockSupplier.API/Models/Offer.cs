namespace MockSupplier.API.Models;

public sealed class Offer
{
    public string Id { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "RUB";
    public int MinOrderQty { get; set; } = 1;
    public int LeadTimeDays { get; set; }
    public bool InStock { get; set; } = true;
    public string? Sku { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class OfferWithSupplier
{
    public string OfferId { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "RUB";
    public int MinOrderQty { get; set; }
    public int LeadTimeDays { get; set; }
    public bool InStock { get; set; }
    public string? Sku { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SupplierInfo Supplier { get; set; } = null!;
}

public sealed class SupplierInfo
{
    public string Id { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public double Rating { get; set; }
}
