using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Catalog.API.Entities;

public sealed class SupplierMaterial : Entity<Guid>
{
    public Guid MaterialId { get; private set; }
    public Guid SupplierId { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "RUB";
    public int MinOrderQty { get; private set; } = 1;
    public int LeadTimeDays { get; private set; }
    public bool InStock { get; private set; } = true;
    public string? Sku { get; private set; }
    public string? Notes { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SupplierMaterial() { }

    public static SupplierMaterial Create(
        Guid id,
        Guid materialId,
        Guid supplierId,
        decimal price,
        string currency = "RUB",
        int minOrderQty = 1,
        int leadTimeDays = 7,
        bool inStock = true,
        string? sku = null,
        string? notes = null)
    {
        return new SupplierMaterial
        {
            Id = id,
            MaterialId = materialId,
            SupplierId = supplierId,
            Price = price,
            Currency = currency,
            MinOrderQty = minOrderQty,
            LeadTimeDays = leadTimeDays,
            InStock = inStock,
            Sku = sku,
            Notes = notes,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal price, string currency)
    {
        Price = price;
        Currency = currency;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(bool inStock, int leadTimeDays)
    {
        InStock = inStock;
        LeadTimeDays = leadTimeDays;
        UpdatedAt = DateTime.UtcNow;
    }
}
