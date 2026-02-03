using Catalog.API.Contracts;
using Catalog.API.Entities;
using Catalog.API.Persistence;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        // Material Categories
        var materialCategories = app.MapGroup("/material-categories").WithTags("Material Categories");
        materialCategories.MapGet("/", GetMaterialCategories).RequireAuthorization("Permission:catalog:read");
        materialCategories.MapGet("/tree", GetMaterialCategoriesTree).RequireAuthorization("Permission:catalog:read");

        // Materials
        var materials = app.MapGroup("/materials").WithTags("Materials");
        materials.MapGet("/", GetMaterials).RequireAuthorization("Permission:catalog:read");
        materials.MapGet("/{id:guid}", GetMaterial).RequireAuthorization("Permission:catalog:read");
        materials.MapPost("/", CreateMaterial).RequireAuthorization("Permission:catalog:write");
        materials.MapPut("/{id:guid}", UpdateMaterial).RequireAuthorization("Permission:catalog:write");
        materials.MapDelete("/{id:guid}", DeleteMaterial).RequireAuthorization("Permission:catalog:delete");

        // Module Categories
        var moduleCategories = app.MapGroup("/module-categories").WithTags("Module Categories");
        moduleCategories.MapGet("/", GetModuleCategories).RequireAuthorization("Permission:catalog:read");

        // Module Types
        var moduleTypes = app.MapGroup("/module-types").WithTags("Module Types");
        moduleTypes.MapGet("/", GetModuleTypes).RequireAuthorization("Permission:catalog:read");
        moduleTypes.MapGet("/count", GetModuleTypesCount).RequireAuthorization("Permission:catalog:read");
        moduleTypes.MapGet("/{id:int}", GetModuleType).RequireAuthorization("Permission:catalog:read");

        // Hardware
        var hardware = app.MapGroup("/hardware").WithTags("Hardware");
        hardware.MapGet("/", GetHardware).RequireAuthorization("Permission:catalog:read");
        hardware.MapGet("/{id:int}", GetHardwareItem).RequireAuthorization("Permission:catalog:read");
    }

    // Material Categories
    private static async Task<IResult> GetMaterialCategories(
        [FromQuery] bool? activeOnly,
        CatalogDbContext dbContext,
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

    private static async Task<IResult> GetMaterialCategoriesTree(
        [FromQuery] bool? activeOnly,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MaterialCategories.AsQueryable();

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        var allCategories = await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.OrderIndex)
            .ToListAsync(cancellationToken);

        // Build tree structure
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
        CatalogDbContext dbContext,
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
        CatalogDbContext dbContext,
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
        CatalogDbContext dbContext,
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
        CatalogDbContext dbContext,
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
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var material = await dbContext.Materials.FindAsync([id], cancellationToken);
        if (material is null)
            return Results.Ok(ApiResponse.Fail("Material not found"));

        dbContext.Materials.Remove(material);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Material deleted successfully"));
    }

    // Module Categories
    private static async Task<IResult> GetModuleCategories(
        [FromQuery] bool? activeOnly,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ModuleCategories.AsQueryable();

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.OrderIndex)
            .Select(c => new ModuleCategoryDto(
                c.Id,
                c.ParentId,
                c.Name,
                c.Description,
                c.OrderIndex,
                c.IsActive))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<ModuleCategoryDto>>.Ok(categories));
    }

    // Module Types
    private static async Task<IResult> GetModuleTypes(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? limit,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ModuleTypes.AsQueryable();

        if (!string.IsNullOrEmpty(categoryId))
            query = query.Where(m => m.CategoryId == categoryId);

        if (activeOnly == true)
            query = query.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(searchLower) ||
                                     m.Type.ToLower().Contains(searchLower));
        }

        // Pagination (optional)
        if (page.HasValue && limit.HasValue)
        {
            var total = await query.CountAsync(cancellationToken);
            var currentPage = page.Value;
            var currentLimit = limit.Value;

            var items = await query
                .Skip((currentPage - 1) * currentLimit)
                .Take(currentLimit)
                .Select(m => new ModuleTypeDto(
                    m.Id,
                    m.CategoryId,
                    m.Type,
                    m.Name,
                    m.WidthDefault,
                    m.WidthMin,
                    m.WidthMax,
                    m.HeightDefault,
                    m.HeightMin,
                    m.HeightMax,
                    m.DepthDefault,
                    m.DepthMin,
                    m.DepthMax,
                    m.PanelThickness,
                    m.BackPanelThickness,
                    m.FacadeThickness,
                    m.FacadeGap,
                    m.FacadeOffset,
                    m.ShelfSideGap,
                    m.ShelfRearInset,
                    m.ShelfFrontInset,
                    m.IsActive))
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<PaginatedResponse<ModuleTypeDto>>.Ok(
                new PaginatedResponse<ModuleTypeDto>(total, currentPage, currentLimit, items)));
        }

        var moduleTypes = await query
            .Select(m => new ModuleTypeDto(
                m.Id,
                m.CategoryId,
                m.Type,
                m.Name,
                m.WidthDefault,
                m.WidthMin,
                m.WidthMax,
                m.HeightDefault,
                m.HeightMin,
                m.HeightMax,
                m.DepthDefault,
                m.DepthMin,
                m.DepthMax,
                m.PanelThickness,
                m.BackPanelThickness,
                m.FacadeThickness,
                m.FacadeGap,
                m.FacadeOffset,
                m.ShelfSideGap,
                m.ShelfRearInset,
                m.ShelfFrontInset,
                m.IsActive))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<ModuleTypeDto>>.Ok(moduleTypes));
    }

    private static async Task<IResult> GetModuleTypesCount(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ModuleTypes.AsQueryable();

        if (!string.IsNullOrEmpty(categoryId))
            query = query.Where(m => m.CategoryId == categoryId);

        if (activeOnly == true)
            query = query.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(searchLower) ||
                                     m.Type.ToLower().Contains(searchLower));
        }

        var count = await query.CountAsync(cancellationToken);

        return Results.Ok(ApiResponse<object>.Ok(new { count }));
    }

    private static async Task<IResult> GetModuleType(
        int id,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var moduleType = await dbContext.ModuleTypes.FindAsync([id], cancellationToken);
        if (moduleType is null)
            return Results.Ok(ApiResponse<ModuleTypeDto>.Fail("Module type not found"));

        return Results.Ok(ApiResponse<ModuleTypeDto>.Ok(new ModuleTypeDto(
            moduleType.Id,
            moduleType.CategoryId,
            moduleType.Type,
            moduleType.Name,
            moduleType.WidthDefault,
            moduleType.WidthMin,
            moduleType.WidthMax,
            moduleType.HeightDefault,
            moduleType.HeightMin,
            moduleType.HeightMax,
            moduleType.DepthDefault,
            moduleType.DepthMin,
            moduleType.DepthMax,
            moduleType.PanelThickness,
            moduleType.BackPanelThickness,
            moduleType.FacadeThickness,
            moduleType.FacadeGap,
            moduleType.FacadeOffset,
            moduleType.ShelfSideGap,
            moduleType.ShelfRearInset,
            moduleType.ShelfFrontInset,
            moduleType.IsActive)));
    }

    // Hardware
    private static async Task<IResult> GetHardware(
        [FromQuery] string? type,
        [FromQuery] bool? activeOnly,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Hardware.AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(h => h.Type == type);

        if (activeOnly == true)
            query = query.Where(h => h.IsActive);

        var hardware = await query
            .Select(h => new HardwareDto(
                h.Id,
                h.Type,
                h.Name,
                h.Brand,
                h.Model,
                h.ModelUrl,
                h.Params,
                h.IsActive))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<HardwareDto>>.Ok(hardware));
    }

    private static async Task<IResult> GetHardwareItem(
        int id,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var hw = await dbContext.Hardware.FindAsync([id], cancellationToken);
        if (hw is null)
            return Results.Ok(ApiResponse<HardwareDto>.Fail("Hardware not found"));

        return Results.Ok(ApiResponse<HardwareDto>.Ok(new HardwareDto(
            hw.Id,
            hw.Type,
            hw.Name,
            hw.Brand,
            hw.Model,
            hw.ModelUrl,
            hw.Params,
            hw.IsActive)));
    }
}
