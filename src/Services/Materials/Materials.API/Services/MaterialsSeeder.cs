using System.Text.Json;
using Materials.API.Entities;
using Materials.API.Persistence;

namespace Materials.API.Services;

public sealed class MaterialsSeeder
{
    private readonly MaterialsDbContext _dbContext;
    private readonly ILogger<MaterialsSeeder> _logger;
    private readonly IWebHostEnvironment _environment;

    public MaterialsSeeder(MaterialsDbContext dbContext, ILogger<MaterialsSeeder> logger, IWebHostEnvironment environment)
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

            _logger.LogInformation("Materials seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during materials seeding");
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
}

#region Seed Data Models

internal sealed class SeedData
{
    public List<MaterialCategorySeed>? MaterialCategories { get; set; }
    public List<MaterialSeed>? Materials { get; set; }
    public List<SupplierSeed>? Suppliers { get; set; }
    public List<SupplierMaterialSeed>? SupplierMaterials { get; set; }
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
}

#endregion
