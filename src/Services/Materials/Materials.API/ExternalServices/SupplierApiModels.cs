using System.Text.Json.Serialization;

namespace Materials.API.ExternalServices;

// Response wrapper from external API
public sealed class ExternalApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

// Category from external API
public sealed class ExternalCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("orderIndex")]
    public int OrderIndex { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

// Category tree node from external API
public sealed class ExternalCategoryTreeNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("orderIndex")]
    public int OrderIndex { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("supplierIds")]
    public List<string> SupplierIds { get; set; } = [];

    [JsonPropertyName("children")]
    public List<ExternalCategoryTreeNode> Children { get; set; } = [];
}

// Material from external API
public sealed class ExternalMaterial
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "sqm";

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("textureUrl")]
    public string? TextureUrl { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

// Supplier from external API
public sealed class ExternalSupplier
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("contactName")]
    public string? ContactName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

// Supplier info in offer
public sealed class ExternalSupplierInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("contactName")]
    public string? ContactName { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }
}

// Offer with supplier info
public sealed class ExternalOfferWithSupplier
{
    [JsonPropertyName("offerId")]
    public string OfferId { get; set; } = string.Empty;

    [JsonPropertyName("materialId")]
    public string MaterialId { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "RUB";

    [JsonPropertyName("minOrderQty")]
    public int MinOrderQty { get; set; }

    [JsonPropertyName("leadTimeDays")]
    public int LeadTimeDays { get; set; }

    [JsonPropertyName("inStock")]
    public bool InStock { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("supplier")]
    public ExternalSupplierInfo Supplier { get; set; } = null!;
}

// Offers response from external API
public sealed class ExternalOffersData
{
    [JsonPropertyName("material")]
    public ExternalMaterialBrief Material { get; set; } = null!;

    [JsonPropertyName("offers")]
    public List<ExternalOfferWithSupplier> Offers { get; set; } = [];

    [JsonPropertyName("totalOffers")]
    public int TotalOffers { get; set; }
}

public sealed class ExternalMaterialBrief
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
