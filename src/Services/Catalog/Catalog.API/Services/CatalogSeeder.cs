using System.Text.Json;
using Catalog.API.Entities;
using Catalog.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Services;

public sealed class CatalogSeeder
{
    private readonly CatalogDbContext _dbContext;
    private readonly ILogger<CatalogSeeder> _logger;
    private readonly IWebHostEnvironment _environment;

    public CatalogSeeder(CatalogDbContext dbContext, ILogger<CatalogSeeder> logger, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _logger = logger;
        _environment = environment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedDataPath = Path.Combine(_environment.ContentRootPath, "seed-data", "db.json");

        if (!File.Exists(seedDataPath))
        {
            _logger.LogWarning("Seed data file not found at {Path}", seedDataPath);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(seedDataPath, cancellationToken);
            var seedData = JsonSerializer.Deserialize<SeedData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (seedData is null)
            {
                _logger.LogWarning("Failed to deserialize seed data");
                return;
            }

            await SeedMaterialCategoriesAsync(seedData.MaterialCategories, cancellationToken);
            await SeedMaterialsAsync(seedData.Materials, cancellationToken);
            await SeedSuppliersAsync(seedData.Suppliers, cancellationToken);
            await SeedSupplierMaterialsAsync(seedData.SupplierMaterials, cancellationToken);
            await SeedModuleCategoriesAsync(seedData.ModuleTypes, cancellationToken);
            await SeedModuleTypesAsync(seedData.ModuleTypes, cancellationToken);
            await SeedHardwareAsync(seedData.Hardware, cancellationToken);
            await SeedModuleHardwareAsync(seedData.ModuleHardware, cancellationToken);

            _logger.LogInformation("Catalog seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during catalog seeding");
            throw;
        }
    }

    private async Task SeedMaterialCategoriesAsync(List<MaterialCategorySeed>? categories, CancellationToken cancellationToken)
    {
        if (categories is null || !categories.Any())
            return;

        var entities = categories.Select(c => MaterialCategory.Create(
            Guid.Parse(c.Id),
            c.Name,
            string.IsNullOrEmpty(c.ParentId) ? null : Guid.Parse(c.ParentId),
            c.Level,
            c.Order
        )).ToList();

        _dbContext.MaterialCategories.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} material categories", entities.Count);
    }

    private async Task SeedMaterialsAsync(List<MaterialSeed>? materials, CancellationToken cancellationToken)
    {
        if (materials is null || !materials.Any())
            return;

        var entities = materials.Select(m => Material.Create(
            Guid.Parse(m.Id),
            Guid.Parse(m.CategoryId),
            m.Name,
            m.Description,
            m.Unit ?? "sqm",
            m.Color,
            m.TextureUrl
        )).ToList();

        _dbContext.Materials.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} materials", entities.Count);
    }

    private async Task SeedSuppliersAsync(List<SupplierSeed>? suppliers, CancellationToken cancellationToken)
    {
        if (suppliers is null || !suppliers.Any())
            return;

        var entities = suppliers.Select(s => Supplier.Create(
            Guid.Parse(s.Id),
            s.Company,
            s.FirstName,
            s.LastName,
            s.Email,
            s.Phone
        )).ToList();

        _dbContext.Suppliers.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} suppliers", entities.Count);
    }

    private async Task SeedSupplierMaterialsAsync(List<SupplierMaterialSeed>? supplierMaterials, CancellationToken cancellationToken)
    {
        if (supplierMaterials is null || !supplierMaterials.Any())
            return;

        var entities = supplierMaterials.Select(sm => SupplierMaterial.Create(
            Guid.Parse(sm.Id),
            Guid.Parse(sm.MaterialId),
            Guid.Parse(sm.SupplierId),
            sm.Price,
            sm.Currency ?? "RUB",
            sm.MinOrderQty,
            sm.LeadTimeDays,
            sm.InStock,
            sm.Sku
        )).ToList();

        _dbContext.SupplierMaterials.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} supplier materials", entities.Count);
    }

    private async Task SeedModuleCategoriesAsync(List<ModuleTypeSeed>? moduleTypes, CancellationToken cancellationToken)
    {
        if (moduleTypes is null || !moduleTypes.Any())
            return;

        // Extract unique categories from module types
        var categories = moduleTypes
            .Select(m => m.CategoryId)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .Select((categoryId, index) => ModuleCategory.Create(
                categoryId!,
                FormatCategoryName(categoryId!),
                null,
                null,
                index
            ))
            .ToList();

        _dbContext.ModuleCategories.AddRange(categories);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} module categories", categories.Count);
    }

    private static string FormatCategoryName(string categoryId)
    {
        // Convert snake_case to Title Case (e.g., "base_single_door" -> "Base Single Door")
        return string.Join(" ", categoryId.Split('_')
            .Select(w => char.ToUpper(w[0]) + w[1..]));
    }

    private async Task SeedModuleTypesAsync(List<ModuleTypeSeed>? moduleTypes, CancellationToken cancellationToken)
    {
        if (moduleTypes is null || !moduleTypes.Any())
            return;

        var entities = moduleTypes.Select(m =>
        {
            var @params = m.Params;
            return ModuleType.Create(
                m.Id,
                m.CategoryId ?? "unknown",
                m.Type,
                m.Name,
                @params?.Width?.Default ?? 600,
                @params?.Width?.Min ?? 300,
                @params?.Width?.Max ?? 1200,
                @params?.Height?.Default ?? 720,
                @params?.Height?.Min ?? 720,
                @params?.Height?.Max ?? 720,
                @params?.Depth?.Default ?? 560,
                @params?.Depth?.Min ?? 500,
                @params?.Depth?.Max ?? 600,
                @params?.PanelThickness?.Default ?? 16,
                @params?.BackPanelThickness?.Default ?? 4,
                @params?.FacadeThickness?.Default ?? 18,
                @params?.FacadeGap?.Default ?? 2,
                0,
                @params?.ShelfSideGap?.Default ?? 2,
                @params?.ShelfRearInset?.Default ?? 20,
                @params?.ShelfFrontInset?.Default ?? 10
            );
        }).ToList();

        _dbContext.ModuleTypes.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} module types", entities.Count);
    }

    private async Task SeedHardwareAsync(List<HardwareSeed>? hardware, CancellationToken cancellationToken)
    {
        if (hardware is null || !hardware.Any())
            return;

        var entities = hardware.Select(h =>
        {
            var paramsJson = h.Params is not null
                ? JsonSerializer.Serialize(h.Params)
                : null;

            return Hardware.Create(
                h.Id,
                h.Type,
                h.Name,
                h.Params?.GetValueOrDefault("brand")?.ToString(),
                h.Params?.GetValueOrDefault("model")?.ToString(),
                h.ModelUrl,
                paramsJson
            );
        }).ToList();

        _dbContext.Hardware.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} hardware items", entities.Count);
    }

    private async Task SeedModuleHardwareAsync(List<ModuleHardwareSeed>? moduleHardware, CancellationToken cancellationToken)
    {
        if (moduleHardware is null || !moduleHardware.Any())
            return;

        var entities = moduleHardware.Select(mh => ModuleHardware.Create(
            mh.Id,
            mh.ModuleTypeId,
            mh.HardwareId,
            mh.Role,
            mh.Quantity ?? "1",
            mh.Position?.X,
            mh.Position?.Y,
            mh.Position?.Z
        )).ToList();

        _dbContext.ModuleHardware.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} module hardware relations", entities.Count);
    }
}

#region Seed Data Models

internal sealed class SeedData
{
    public List<MaterialCategorySeed>? MaterialCategories { get; set; }
    public List<MaterialSeed>? Materials { get; set; }
    public List<SupplierSeed>? Suppliers { get; set; }
    public List<SupplierMaterialSeed>? SupplierMaterials { get; set; }
    public List<ModuleTypeSeed>? ModuleTypes { get; set; }
    public List<HardwareSeed>? Hardware { get; set; }
    public List<ModuleHardwareSeed>? ModuleHardware { get; set; }
}

internal sealed class MaterialCategorySeed
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public int Level { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
}

internal sealed class MaterialSeed
{
    public string Id { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public string? Color { get; set; }
    public string? TextureUrl { get; set; }
}

internal sealed class SupplierSeed
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Company { get; set; } = string.Empty;
}

internal sealed class SupplierMaterialSeed
{
    public string Id { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public int MinOrderQty { get; set; } = 1;
    public int LeadTimeDays { get; set; }
    public bool InStock { get; set; } = true;
    public string? Sku { get; set; }
    public string? Notes { get; set; }
}

internal sealed class ModuleTypeSeed
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
    public ModuleParamsSeed? Params { get; set; }
}

internal sealed class ModuleParamsSeed
{
    public DimensionSeed? Width { get; set; }
    public DimensionSeed? Height { get; set; }
    public DimensionSeed? Depth { get; set; }
    public DefaultValueSeed? PanelThickness { get; set; }
    public DefaultValueSeed? BackPanelThickness { get; set; }
    public DefaultValueSeed? FacadeThickness { get; set; }
    public DefaultValueSeed? FacadeGap { get; set; }
    public DefaultValueSeed? ShelfSideGap { get; set; }
    public DefaultValueSeed? ShelfRearInset { get; set; }
    public DefaultValueSeed? ShelfFrontInset { get; set; }
}

internal sealed class DimensionSeed
{
    public int Default { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
    public int? Step { get; set; }
}

internal sealed class DefaultValueSeed
{
    public int Default { get; set; }
}

internal sealed class HardwareSeed
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ModelUrl { get; set; }
    public Dictionary<string, object>? Params { get; set; }
}

internal sealed class ModuleHardwareSeed
{
    public int Id { get; set; }
    public int ModuleTypeId { get; set; }
    public int HardwareId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Quantity { get; set; }
    public PositionFormulaSeed? Position { get; set; }
}

internal sealed class PositionFormulaSeed
{
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Z { get; set; }
}

#endregion
