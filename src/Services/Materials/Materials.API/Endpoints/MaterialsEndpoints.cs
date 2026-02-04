using Domeo.Shared.Contracts;
using Materials.API.Contracts;
using Materials.API.ExternalServices;
using Microsoft.AspNetCore.Mvc;

namespace Materials.API.Endpoints;

public static class MaterialsEndpoints
{
    public static void MapMaterialsEndpoints(this IEndpointRouteBuilder app)
    {
        // Categories
        var categories = app.MapGroup("/categories").WithTags("Categories");
        categories.MapGet("/tree", GetCategoriesTree).RequireAuthorization("Permission:catalog:read");

        // Suppliers
        var suppliers = app.MapGroup("/suppliers").WithTags("Suppliers");
        suppliers.MapGet("/", GetSuppliers).RequireAuthorization("Permission:suppliers:read");
        suppliers.MapGet("/{id}", GetSupplier).RequireAuthorization("Permission:suppliers:read");

        // Items (Materials)
        var items = app.MapGroup("/items").WithTags("Materials");
        items.MapGet("/", GetMaterials).RequireAuthorization("Permission:catalog:read");
        items.MapGet("/{id}", GetMaterial).RequireAuthorization("Permission:catalog:read");
        items.MapGet("/{id}/offers", GetMaterialOffers).RequireAuthorization("Permission:catalog:read");
    }

    // GET /categories/tree - Дерево категорий с supplierIds
    private static async Task<IResult> GetCategoriesTree(
        [FromQuery] bool? activeOnly,
        ISupplierApiClient supplierApi,
        CancellationToken ct)
    {
        try
        {
            var externalTree = await supplierApi.GetCategoriesTreeAsync(activeOnly ?? true, ct);

            var tree = externalTree.Select(MapCategoryTreeNode).ToList();

            return Results.Ok(ApiResponse<List<CategoryTreeNodeDto>>.Ok(tree));
        }
        catch (HttpRequestException ex)
        {
            return Results.Ok(ApiResponse<List<CategoryTreeNodeDto>>.Fail($"Supplier service unavailable: {ex.Message}"));
        }
    }

    // GET /suppliers - Справочник поставщиков
    private static async Task<IResult> GetSuppliers(
        [FromQuery] bool? activeOnly,
        ISupplierApiClient supplierApi,
        CancellationToken ct)
    {
        try
        {
            var externalSuppliers = await supplierApi.GetSuppliersAsync(activeOnly ?? true, ct);

            var suppliers = externalSuppliers.Select(s => new SupplierResponseDto(
                s.Id,
                s.Company,
                s.ContactName,
                s.Email,
                s.Phone,
                s.Address,
                s.Website,
                s.Rating,
                s.IsActive)).ToList();

            return Results.Ok(ApiResponse<List<SupplierResponseDto>>.Ok(suppliers));
        }
        catch (HttpRequestException ex)
        {
            return Results.Ok(ApiResponse<List<SupplierResponseDto>>.Fail($"Supplier service unavailable: {ex.Message}"));
        }
    }

    // GET /suppliers/{id} - Поставщик по ID
    private static async Task<IResult> GetSupplier(
        string id,
        ISupplierApiClient supplierApi,
        CancellationToken ct)
    {
        try
        {
            var externalSupplier = await supplierApi.GetSupplierAsync(id, ct);

            if (externalSupplier == null)
                return Results.Ok(ApiResponse<SupplierResponseDto>.Fail("Supplier not found"));

            var supplier = new SupplierResponseDto(
                externalSupplier.Id,
                externalSupplier.Company,
                externalSupplier.ContactName,
                externalSupplier.Email,
                externalSupplier.Phone,
                externalSupplier.Address,
                externalSupplier.Website,
                externalSupplier.Rating,
                externalSupplier.IsActive);

            return Results.Ok(ApiResponse<SupplierResponseDto>.Ok(supplier));
        }
        catch (HttpRequestException ex)
        {
            return Results.Ok(ApiResponse<SupplierResponseDto>.Fail($"Supplier service unavailable: {ex.Message}"));
        }
    }

    // GET /items?categoryId=X - Список материалов
    private static async Task<IResult> GetMaterials(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        ISupplierApiClient supplierApi,
        CancellationToken ct)
    {
        try
        {
            var externalMaterials = await supplierApi.GetMaterialsAsync(categoryId, activeOnly ?? true, ct);

            var materials = externalMaterials.Select(m => new MaterialResponseDto(
                m.Id,
                m.CategoryId,
                m.Name,
                m.Description,
                m.Unit,
                m.Color,
                m.TextureUrl,
                m.IsActive)).ToList();

            return Results.Ok(ApiResponse<List<MaterialResponseDto>>.Ok(materials));
        }
        catch (HttpRequestException ex)
        {
            return Results.Ok(ApiResponse<List<MaterialResponseDto>>.Fail($"Supplier service unavailable: {ex.Message}"));
        }
    }

    // GET /items/{id} - Материал по ID
    private static async Task<IResult> GetMaterial(
        string id,
        ISupplierApiClient supplierApi,
        CancellationToken ct)
    {
        try
        {
            var externalMaterial = await supplierApi.GetMaterialAsync(id, ct);

            if (externalMaterial == null)
                return Results.Ok(ApiResponse<MaterialResponseDto>.Fail("Material not found"));

            var material = new MaterialResponseDto(
                externalMaterial.Id,
                externalMaterial.CategoryId,
                externalMaterial.Name,
                externalMaterial.Description,
                externalMaterial.Unit,
                externalMaterial.Color,
                externalMaterial.TextureUrl,
                externalMaterial.IsActive);

            return Results.Ok(ApiResponse<MaterialResponseDto>.Ok(material));
        }
        catch (HttpRequestException ex)
        {
            return Results.Ok(ApiResponse<MaterialResponseDto>.Fail($"Supplier service unavailable: {ex.Message}"));
        }
    }

    // GET /items/{id}/offers - Предложения для материала
    private static async Task<IResult> GetMaterialOffers(
        string id,
        ISupplierApiClient supplierApi,
        CancellationToken ct)
    {
        try
        {
            var externalOffers = await supplierApi.GetOffersAsync(id, ct);

            if (externalOffers == null)
                return Results.Ok(ApiResponse<MaterialOffersResponseDto>.Fail("Material not found"));

            var response = new MaterialOffersResponseDto
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

            return Results.Ok(ApiResponse<MaterialOffersResponseDto>.Ok(response));
        }
        catch (HttpRequestException ex)
        {
            return Results.Ok(ApiResponse<MaterialOffersResponseDto>.Fail($"Supplier service unavailable: {ex.Message}"));
        }
    }

    private static CategoryTreeNodeDto MapCategoryTreeNode(ExternalCategoryTreeNode node)
    {
        return new CategoryTreeNodeDto
        {
            Id = node.Id,
            ParentId = node.ParentId,
            Name = node.Name,
            Level = node.Level,
            OrderIndex = node.OrderIndex,
            IsActive = node.IsActive,
            SupplierIds = node.SupplierIds,
            Children = node.Children.Select(MapCategoryTreeNode).ToList()
        };
    }
}
