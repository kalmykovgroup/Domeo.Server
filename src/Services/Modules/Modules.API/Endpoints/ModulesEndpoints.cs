using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.API.Persistence;

namespace Modules.API.Endpoints;

public static class ModulesEndpoints
{
    public static void MapModulesEndpoints(this IEndpointRouteBuilder app)
    {
        // Categories
        var categories = app.MapGroup("/categories").WithTags("Module Categories");
        categories.MapGet("/", GetCategories).RequireAuthorization("Permission:catalog:read");
        categories.MapGet("/tree", GetCategoriesTree).RequireAuthorization("Permission:catalog:read");

        // Types (3D modules)
        var types = app.MapGroup("/types").WithTags("Module Types");
        types.MapGet("/", GetModuleTypes).RequireAuthorization("Permission:catalog:read");
        types.MapGet("/count", GetModuleTypesCount).RequireAuthorization("Permission:catalog:read");
        types.MapGet("/{id:int}", GetModuleType).RequireAuthorization("Permission:catalog:read");

        // Hardware
        var hardware = app.MapGroup("/hardware").WithTags("Hardware");
        hardware.MapGet("/", GetHardware).RequireAuthorization("Permission:catalog:read");
        hardware.MapGet("/{id:int}", GetHardwareItem).RequireAuthorization("Permission:catalog:read");
    }

    // Categories
    private static async Task<IResult> GetCategories(
        [FromQuery] bool? activeOnly,
        ModulesDbContext dbContext,
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

    private static async Task<IResult> GetCategoriesTree(
        [FromQuery] bool? activeOnly,
        ModulesDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ModuleCategories.AsQueryable();

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        var allCategories = await query
            .OrderBy(c => c.OrderIndex)
            .ToListAsync(cancellationToken);

        // Build tree structure
        var categoryDict = allCategories.ToDictionary(
            c => c.Id,
            c => new ModuleCategoryTreeDto(c.Id, c.ParentId, c.Name, c.Description, c.OrderIndex, c.IsActive));

        var rootCategories = new List<ModuleCategoryTreeDto>();

        foreach (var category in allCategories)
        {
            var treeNode = categoryDict[category.Id];

            if (category.ParentId is not null && categoryDict.TryGetValue(category.ParentId, out var parent))
            {
                parent.Children.Add(treeNode);
            }
            else
            {
                rootCategories.Add(treeNode);
            }
        }

        return Results.Ok(ApiResponse<List<ModuleCategoryTreeDto>>.Ok(rootCategories));
    }

    // Module Types
    private static async Task<IResult> GetModuleTypes(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? limit,
        ModulesDbContext dbContext,
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
                .Select(m => ToDto(m))
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<PaginatedResponse<ModuleTypeDto>>.Ok(
                new PaginatedResponse<ModuleTypeDto>(total, currentPage, currentLimit, items)));
        }

        var moduleTypes = await query
            .Select(m => ToDto(m))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<ModuleTypeDto>>.Ok(moduleTypes));
    }

    private static async Task<IResult> GetModuleTypesCount(
        [FromQuery] string? categoryId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        ModulesDbContext dbContext,
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
        ModulesDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var moduleType = await dbContext.ModuleTypes.FindAsync([id], cancellationToken);
        if (moduleType is null)
            return Results.Ok(ApiResponse<ModuleTypeDto>.Fail("Module type not found"));

        return Results.Ok(ApiResponse<ModuleTypeDto>.Ok(ToDto(moduleType)));
    }

    // Hardware
    private static async Task<IResult> GetHardware(
        [FromQuery] string? type,
        [FromQuery] bool? activeOnly,
        ModulesDbContext dbContext,
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
        ModulesDbContext dbContext,
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

    private static ModuleTypeDto ToDto(Entities.ModuleType m) => new(
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
        m.IsActive);
}
