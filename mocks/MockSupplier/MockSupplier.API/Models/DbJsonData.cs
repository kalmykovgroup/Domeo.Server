namespace MockSupplier.API.Models;

public class DbJsonData
{
    public List<DbMaterialCategory>? MaterialCategories { get; set; }
    public List<DbMaterial>? Materials { get; set; }
    public List<DbSupplier>? Suppliers { get; set; }
    public List<DbSupplierMaterial>? SupplierMaterials { get; set; }
    public List<DbBrand>? Brands { get; set; }
    public List<DbCategoryAttribute>? CategoryAttributes { get; set; }
    public List<DbMaterialAttribute>? MaterialAttributes { get; set; }
}

public class DbMaterialCategory
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ParentId { get; set; }
    public int Level { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string>? SupplierIds { get; set; }
}

public class DbMaterial
{
    public string Id { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string? BrandId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Unit { get; set; } = "sqm";
    public string? Color { get; set; }
    public string? TextureUrl { get; set; }
}

public class DbSupplier
{
    public string Id { get; set; } = "";
    public string Company { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class DbSupplierMaterial
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

public class DbBrand
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DbCategoryAttribute
{
    public string Id { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public string? Unit { get; set; }
    public List<string>? EnumValues { get; set; }
}

public class DbMaterialAttribute
{
    public string MaterialId { get; set; } = "";
    public string AttributeId { get; set; } = "";
    public string Value { get; set; } = "";
}
