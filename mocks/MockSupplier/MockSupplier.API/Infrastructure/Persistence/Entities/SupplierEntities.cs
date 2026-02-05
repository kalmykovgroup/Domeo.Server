namespace MockSupplier.API.Infrastructure.Persistence.Entities;

public class CategoryEntity
{
    public string Id { get; set; } = "";
    public string? ParentId { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
}

public class MaterialEntity
{
    public string Id { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string? BrandId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Unit { get; set; } = "sqm";
    public string? Color { get; set; }
    public string? TextureUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SupplierEntity
{
    public string Id { get; set; } = "";
    public string Company { get; set; } = "";
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}

public class OfferEntity
{
    public string Id { get; set; } = "";
    public string MaterialId { get; set; } = "";
    public string SupplierId { get; set; } = "";
    public decimal Price { get; set; }
    public string Currency { get; set; } = "RUB";
    public int MinOrderQty { get; set; } = 1;
    public int LeadTimeDays { get; set; }
    public bool InStock { get; set; } = true;
    public string? Sku { get; set; }
    public string? Notes { get; set; }
}

public class BrandEntity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CategoryAttributeEntity
{
    public string Id { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public string? Unit { get; set; }
    public string? EnumValuesJson { get; set; }
}

public class MaterialAttributeValueEntity
{
    public int Id { get; set; }
    public string MaterialId { get; set; } = "";
    public string AttributeId { get; set; } = "";
    public string Value { get; set; } = "";
}
