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

        // Materials
        var materials = app.MapGroup("/api/materials")
            .WithTags("Materials");

        materials.MapGet("/", GetMaterials)
            .WithName("GetMaterials")
            .WithSummary("Получить список материалов");

        materials.MapGet("/{id}", GetMaterial)
            .WithName("GetMaterial")
            .WithSummary("Получить материал по ID");

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

    private static IResult GetMaterials(DataStore store, string? categoryId = null, bool activeOnly = true)
    {
        var materials = store.GetMaterials(categoryId, activeOnly);
        return Results.Ok(new { success = true, data = materials });
    }

    private static IResult GetMaterial(string id, DataStore store)
    {
        var material = store.GetMaterial(id);
        if (material == null)
            return Results.NotFound(new { success = false, message = "Material not found" });

        return Results.Ok(new { success = true, data = material });
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
