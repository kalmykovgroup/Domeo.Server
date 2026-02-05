using MockSupplier.API.Services;

namespace MockSupplier.API.Endpoints;

public static class SupplierEndpoints
{
    public static void MapSupplierEndpoints(this WebApplication app)
    {
        // Categories
        var categories = app.MapGroup("/api/categories")
            .WithTags("Categories");

        categories.MapGet("/", GetCategories)
            .WithName("GetCategories")
            .WithSummary("Получить список категорий материалов");

        categories.MapGet("/{id}", GetCategory)
            .WithName("GetCategory")
            .WithSummary("Получить категорию по ID");

        categories.MapGet("/tree", GetCategoriesTree)
            .WithName("GetCategoriesTree")
            .WithSummary("Получить дерево категорий с supplierIds");

        categories.MapGet("/{id}/attributes", GetCategoryAttributes)
            .WithName("GetCategoryAttributes")
            .WithSummary("Получить атрибуты категории");

        // Materials
        var materials = app.MapGroup("/api/materials")
            .WithTags("Materials");

        materials.MapGet("/", GetMaterials)
            .WithName("GetMaterials")
            .WithSummary("Получить список материалов");

        materials.MapGet("/suggest", GetSearchSuggestions)
            .WithName("GetSearchSuggestions")
            .WithSummary("Поиск с подсказками по материалам, брендам, категориям и атрибутам");

        materials.MapGet("/{id}", GetMaterial)
            .WithName("GetMaterial")
            .WithSummary("Получить материал по ID");

        // Brands
        var brands = app.MapGroup("/api/brands")
            .WithTags("Brands");

        brands.MapGet("/", GetBrands)
            .WithName("GetBrands")
            .WithSummary("Получить список брендов");

        // Suppliers
        var suppliers = app.MapGroup("/api/suppliers")
            .WithTags("Suppliers");

        suppliers.MapGet("/", GetSuppliers)
            .WithName("GetSuppliers")
            .WithSummary("Получить список поставщиков");

        suppliers.MapGet("/{id}", GetSupplier)
            .WithName("GetSupplier")
            .WithSummary("Получить поставщика по ID");

        suppliers.MapGet("/{id}/offers", GetSupplierOffers)
            .WithName("GetSupplierOffers")
            .WithSummary("Получить предложения поставщика");

        // Offers
        var offers = app.MapGroup("/api/offers")
            .WithTags("Offers");

        offers.MapGet("/", GetOffers)
            .WithName("GetOffers")
            .WithSummary("Получить предложения по материалу (поставщики + цены)");
    }

    private static IResult GetCategories(DataStore store, bool activeOnly = true)
    {
        var categories = store.GetCategories(activeOnly);
        return Results.Ok(new { success = true, data = categories });
    }

    private static IResult GetCategory(string id, DataStore store)
    {
        var category = store.GetCategory(id);
        if (category == null)
            return Results.NotFound(new { success = false, message = "Category not found" });

        return Results.Ok(new { success = true, data = category });
    }

    private static IResult GetCategoriesTree(DataStore store, bool activeOnly = true)
    {
        var tree = store.GetCategoriesTree(activeOnly);
        return Results.Ok(new { success = true, data = tree });
    }

    private static IResult GetCategoryAttributes(string id, DataStore store)
    {
        var category = store.GetCategory(id);
        if (category == null)
            return Results.NotFound(new { success = false, message = "Category not found" });

        var attributes = store.GetCategoryAttributes(id);
        return Results.Ok(new { success = true, data = attributes });
    }

    private static IResult GetSearchSuggestions(DataStore store, string? query = null, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Results.Ok(new { success = true, data = Array.Empty<object>() });

        var suggestions = store.GetSearchSuggestions(query, limit);
        return Results.Ok(new { success = true, data = suggestions });
    }

    private static IResult GetMaterials(
        DataStore store,
        HttpContext httpContext,
        string? categoryId = null,
        bool activeOnly = true,
        string? brandId = null,
        string? supplierId = null)
    {
        // Collect attribute filters from query: attr_{name}={value}
        Dictionary<string, string>? attributes = null;
        foreach (var param in httpContext.Request.Query)
        {
            if (param.Key.StartsWith("attr_", StringComparison.OrdinalIgnoreCase))
            {
                attributes ??= new Dictionary<string, string>();
                var attrName = param.Key[5..]; // Remove "attr_" prefix
                attributes[attrName] = param.Value.ToString();
            }
        }

        var materials = store.GetMaterials(categoryId, activeOnly, brandId, supplierId, attributes);
        return Results.Ok(new { success = true, data = materials });
    }

    private static IResult GetMaterial(string id, DataStore store)
    {
        var material = store.GetMaterial(id);
        if (material == null)
            return Results.NotFound(new { success = false, message = "Material not found" });

        return Results.Ok(new { success = true, data = material });
    }

    private static IResult GetBrands(DataStore store, bool activeOnly = true)
    {
        var brands = store.GetBrands(activeOnly);
        return Results.Ok(new { success = true, data = brands });
    }

    private static IResult GetSuppliers(DataStore store, bool activeOnly = true)
    {
        var suppliers = store.GetSuppliers(activeOnly);
        return Results.Ok(new { success = true, data = suppliers });
    }

    private static IResult GetSupplier(string id, DataStore store)
    {
        var supplier = store.GetSupplier(id);
        if (supplier == null)
            return Results.NotFound(new { success = false, message = "Supplier not found" });

        return Results.Ok(new { success = true, data = supplier });
    }

    private static IResult GetSupplierOffers(string id, DataStore store)
    {
        var supplier = store.GetSupplier(id);
        if (supplier == null)
            return Results.NotFound(new { success = false, message = "Supplier not found" });

        var offers = store.GetOffersBySupplier(id);
        return Results.Ok(new { success = true, data = offers });
    }

    private static IResult GetOffers(DataStore store, string? materialId = null)
    {
        if (string.IsNullOrEmpty(materialId))
            return Results.BadRequest(new { success = false, message = "materialId is required" });

        var material = store.GetMaterial(materialId);
        if (material == null)
            return Results.NotFound(new { success = false, message = "Material not found" });

        var offers = store.GetOffersByMaterial(materialId);
        return Results.Ok(new
        {
            success = true,
            data = new
            {
                material = new
                {
                    material.Id,
                    material.Name,
                    material.Unit,
                    material.Description
                },
                offers,
                totalOffers = offers.Count
            }
        });
    }
}
