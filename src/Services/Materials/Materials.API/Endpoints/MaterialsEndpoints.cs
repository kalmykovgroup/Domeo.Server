using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Materials.API.Contracts;
using Materials.API.Entities;
using Materials.API.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Materials.API.Endpoints;

public static class MaterialsEndpoints
{
    public static void MapMaterialsEndpoints(this IEndpointRouteBuilder app)
    {
        // Categories
        var categories = app.MapGroup("/categories").WithTags("Material Categories");
        categories.MapGet("/", GetCategories).RequireAuthorization("Permission:catalog:read");
        categories.MapGet("/tree", GetCategoriesTree).RequireAuthorization("Permission:catalog:read");

        // Items (Materials)
        var items = app.MapGroup("/items").WithTags("Materials");
        items.MapGet("/", GetMaterials).RequireAuthorization("Permission:catalog:read");
        items.MapGet("/{id:guid}", GetMaterial).RequireAuthorization("Permission:catalog:read");
        items.MapPost("/", CreateMaterial).RequireAuthorization("Permission:catalog:write");
        items.MapPut("/{id:guid}", UpdateMaterial).RequireAuthorization("Permission:catalog:write");
        items.MapDelete("/{id:guid}", DeleteMaterial).RequireAuthorization("Permission:catalog:delete");

        // Suppliers
        var suppliers = app.MapGroup("/suppliers").WithTags("Suppliers");
        suppliers.MapGet("/", GetSuppliers).RequireAuthorization("Permission:suppliers:read");
        suppliers.MapGet("/{id:guid}", GetSupplier).RequireAuthorization("Permission:suppliers:read");
        suppliers.MapPost("/", CreateSupplier).RequireAuthorization("Permission:suppliers:write");
        suppliers.MapPut("/{id:guid}", UpdateSupplier).RequireAuthorization("Permission:suppliers:write");
        suppliers.MapDelete("/{id:guid}", DeleteSupplier).RequireAuthorization("Permission:suppliers:delete");
    }

    // Categories
    private static async Task<IResult> GetCategories(
        [FromQuery] bool? activeOnly,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MaterialCategories.AsQueryable();

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.OrderIndex)
            .Select(c => new MaterialCategoryDto(
                c.Id,
                c.ParentId,
                c.Name,
                c.Level,
                c.OrderIndex,
                c.IsActive))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<MaterialCategoryDto>>.Ok(categories));
    }

    private static async Task<IResult> GetCategoriesTree(
        [FromQuery] bool? activeOnly,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MaterialCategories.AsQueryable();

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        var allCategories = await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.OrderIndex)
            .ToListAsync(cancellationToken);

        var categoryDict = allCategories.ToDictionary(
            c => c.Id,
            c => new MaterialCategoryTreeDto(c.Id, c.ParentId, c.Name, c.Level, c.OrderIndex, c.IsActive));

        var rootCategories = new List<MaterialCategoryTreeDto>();

        foreach (var category in allCategories)
        {
            var treeNode = categoryDict[category.Id];

            if (category.ParentId.HasValue && categoryDict.TryGetValue(category.ParentId.Value, out var parent))
            {
                parent.Children.Add(treeNode);
            }
            else
            {
                rootCategories.Add(treeNode);
            }
        }

        return Results.Ok(ApiResponse<List<MaterialCategoryTreeDto>>.Ok(rootCategories));
    }

    // Materials
    private static async Task<IResult> GetMaterials(
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? activeOnly,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Materials.AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId.Value);

        if (activeOnly == true)
            query = query.Where(m => m.IsActive);

        var materials = await query
            .Select(m => new MaterialDto(
                m.Id,
                m.CategoryId,
                m.Name,
                m.Description,
                m.Unit,
                m.Color,
                m.TextureUrl,
                m.IsActive))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<MaterialDto>>.Ok(materials));
    }

    private static async Task<IResult> GetMaterial(
        Guid id,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var material = await dbContext.Materials.FindAsync([id], cancellationToken);
        if (material is null)
            return Results.Ok(ApiResponse<MaterialDto>.Fail("Material not found"));

        return Results.Ok(ApiResponse<MaterialDto>.Ok(new MaterialDto(
            material.Id,
            material.CategoryId,
            material.Name,
            material.Description,
            material.Unit,
            material.Color,
            material.TextureUrl,
            material.IsActive)));
    }

    private static async Task<IResult> CreateMaterial(
        [FromBody] CreateMaterialRequest request,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var material = Material.Create(
            Guid.NewGuid(),
            request.CategoryId,
            request.Name,
            request.Description,
            request.Unit,
            request.Color,
            request.TextureUrl);

        dbContext.Materials.Add(material);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(material.Id), "Material created successfully"));
    }

    private static async Task<IResult> UpdateMaterial(
        Guid id,
        [FromBody] UpdateMaterialRequest request,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var material = await dbContext.Materials.FindAsync([id], cancellationToken);
        if (material is null)
            return Results.Ok(ApiResponse.Fail("Material not found"));

        material.Update(request.Name, request.Description, request.Color, request.TextureUrl);

        if (request.IsActive)
            material.Activate();
        else
            material.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Material updated successfully"));
    }

    private static async Task<IResult> DeleteMaterial(
        Guid id,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var material = await dbContext.Materials.FindAsync([id], cancellationToken);
        if (material is null)
            return Results.Ok(ApiResponse.Fail("Material not found"));

        dbContext.Materials.Remove(material);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Material deleted successfully"));
    }

    // Suppliers
    private static async Task<IResult> GetSuppliers(
        [FromQuery] bool? activeOnly,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Suppliers.AsQueryable();

        if (activeOnly == true)
            query = query.Where(s => s.IsActive);

        var suppliers = await query
            .Select(s => new SupplierDto(
                s.Id,
                s.Company,
                s.ContactFirstName,
                s.ContactLastName,
                s.Email,
                s.Phone,
                s.Address,
                s.IsActive))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<SupplierDto>>.Ok(suppliers));
    }

    private static async Task<IResult> GetSupplier(
        Guid id,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FindAsync([id], cancellationToken);
        if (supplier is null)
            return Results.Ok(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Results.Ok(ApiResponse<SupplierDto>.Ok(new SupplierDto(
            supplier.Id,
            supplier.Company,
            supplier.ContactFirstName,
            supplier.ContactLastName,
            supplier.Email,
            supplier.Phone,
            supplier.Address,
            supplier.IsActive)));
    }

    private static async Task<IResult> CreateSupplier(
        [FromBody] CreateSupplierRequest request,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            request.Company,
            request.ContactFirstName,
            request.ContactLastName,
            request.Email,
            request.Phone,
            request.Address);

        dbContext.Suppliers.Add(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(supplier.Id), "Supplier created successfully"));
    }

    private static async Task<IResult> UpdateSupplier(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FindAsync([id], cancellationToken);
        if (supplier is null)
            return Results.Ok(ApiResponse.Fail("Supplier not found"));

        supplier.Update(
            request.Company,
            request.ContactFirstName,
            request.ContactLastName,
            request.Email,
            request.Phone,
            request.Address);

        if (request.IsActive)
            supplier.Activate();
        else
            supplier.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Supplier updated successfully"));
    }

    private static async Task<IResult> DeleteSupplier(
        Guid id,
        MaterialsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FindAsync([id], cancellationToken);
        if (supplier is null)
            return Results.Ok(ApiResponse.Fail("Supplier not found"));

        dbContext.Suppliers.Remove(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Supplier deleted successfully"));
    }
}
