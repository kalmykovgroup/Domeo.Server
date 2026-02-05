using System.Text.Json;
using MockSupplier.API.Infrastructure.Persistence.Entities;
using MockSupplier.API.Models;

namespace MockSupplier.API.Infrastructure.Persistence;

public class SupplierDbSeeder(SupplierDbContext db, IConfiguration configuration)
{
    private const string KukhniSnabId = "1d8ec586-6077-421c-8e24-7a2160c84d11";

    public async Task SeedAsync()
    {
        if (db.Suppliers.Any())
            return;

        var dbJsonPath = configuration.GetValue<string>("Data:DbJsonPath");
        if (string.IsNullOrEmpty(dbJsonPath) || !File.Exists(dbJsonPath))
        {
            Console.WriteLine($"[Seeder] db.json not found at: {dbJsonPath}");
            return;
        }

        Console.WriteLine($"[Seeder] Seeding from {dbJsonPath}...");

        var json = await File.ReadAllTextAsync(dbJsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<DbJsonData>(json, options);

        if (data == null)
        {
            Console.WriteLine("[Seeder] Failed to parse db.json");
            return;
        }

        // Categories
        if (data.MaterialCategories is { Count: > 0 })
        {
            db.Categories.AddRange(data.MaterialCategories.Select(c => new CategoryEntity
            {
                Id = c.Id,
                ParentId = c.ParentId,
                Name = c.Name,
                Level = c.Level,
                OrderIndex = c.Order,
                IsActive = c.IsActive
            }));
        }

        // Brands
        if (data.Brands is { Count: > 0 })
        {
            db.Brands.AddRange(data.Brands.Select(b => new BrandEntity
            {
                Id = b.Id,
                Name = b.Name,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive
            }));
        }

        // Materials
        if (data.Materials is { Count: > 0 })
        {
            db.Materials.AddRange(data.Materials.Select(m => new MaterialEntity
            {
                Id = m.Id,
                CategoryId = m.CategoryId,
                BrandId = m.BrandId,
                Name = m.Name,
                Description = m.Description,
                Unit = m.Unit,
                Color = m.Color,
                TextureUrl = m.TextureUrl,
                IsActive = true
            }));
        }

        // Suppliers
        if (data.Suppliers is { Count: > 0 })
        {
            db.Suppliers.AddRange(data.Suppliers.Select(s => new SupplierEntity
            {
                Id = s.Id,
                Company = s.Company,
                ContactName = $"{s.FirstName} {s.LastName}".Trim(),
                Email = s.Email,
                Phone = s.Phone,
                IsActive = true
            }));
        }

        // Offers (existing)
        var existingOffers = new List<OfferEntity>();
        if (data.SupplierMaterials is { Count: > 0 })
        {
            existingOffers = data.SupplierMaterials.Select(sm => new OfferEntity
            {
                Id = sm.Id,
                MaterialId = sm.MaterialId,
                SupplierId = sm.SupplierId,
                Price = sm.Price,
                Currency = sm.Currency,
                MinOrderQty = sm.MinOrderQty,
                LeadTimeDays = sm.LeadTimeDays,
                InStock = sm.InStock,
                Sku = sm.Sku,
                Notes = sm.Notes
            }).ToList();

            db.Offers.AddRange(existingOffers);
        }

        // Category attributes
        if (data.CategoryAttributes is { Count: > 0 })
        {
            db.CategoryAttributes.AddRange(data.CategoryAttributes.Select(a => new CategoryAttributeEntity
            {
                Id = a.Id,
                CategoryId = a.CategoryId,
                Name = a.Name,
                Type = a.Type,
                Unit = a.Unit,
                EnumValuesJson = a.EnumValues != null
                    ? JsonSerializer.Serialize(a.EnumValues)
                    : null
            }));
        }

        // Material attribute values
        if (data.MaterialAttributes is { Count: > 0 })
        {
            db.MaterialAttributeValues.AddRange(data.MaterialAttributes.Select(a => new MaterialAttributeValueEntity
            {
                MaterialId = a.MaterialId,
                AttributeId = a.AttributeId,
                Value = a.Value
            }));
        }

        // Generate offers for КухниСнаб
        var kukhniSnabOffers = GenerateKukhniSnabOffers(existingOffers);
        if (kukhniSnabOffers.Count > 0)
        {
            db.Offers.AddRange(kukhniSnabOffers);
            Console.WriteLine($"[Seeder] Generated {kukhniSnabOffers.Count} offers for КухниСнаб");
        }

        await db.SaveChangesAsync();

        var stats = $"Categories={db.Categories.Count()}, Materials={db.Materials.Count()}, " +
                    $"Suppliers={db.Suppliers.Count()}, Offers={db.Offers.Count()}, " +
                    $"Brands={db.Brands.Count()}";
        Console.WriteLine($"[Seeder] Done. {stats}");
    }

    private static List<OfferEntity> GenerateKukhniSnabOffers(List<OfferEntity> existingOffers)
    {
        // Take every 3rd material that already has offers from other suppliers
        var materialIdsWithOffers = existingOffers
            .Where(o => o.SupplierId != KukhniSnabId)
            .GroupBy(o => o.MaterialId)
            .Where(g => g.Any())
            .Select(g => new
            {
                MaterialId = g.Key,
                ReferencePrice = g.Average(o => o.Price),
                ReferenceCurrency = g.First().Currency,
                ReferenceUnit = g.First().MinOrderQty
            })
            .ToList();

        var result = new List<OfferEntity>();
        var rng = new Random(42); // deterministic seed

        for (var i = 0; i < materialIdsWithOffers.Count; i += 3)
        {
            var mat = materialIdsWithOffers[i];
            var priceVariation = 0.85m + (decimal)(rng.NextDouble() * 0.30); // ±15% range
            var price = Math.Round(mat.ReferencePrice * priceVariation, 0);

            result.Add(new OfferEntity
            {
                Id = $"ks-offer-{i / 3 + 1:D4}",
                MaterialId = mat.MaterialId,
                SupplierId = KukhniSnabId,
                Price = price,
                Currency = mat.ReferenceCurrency,
                MinOrderQty = rng.Next(1, 10),
                LeadTimeDays = rng.Next(2, 8),
                InStock = rng.NextDouble() > 0.2,
                Sku = $"KS-{i / 3 + 1:D4}",
                Notes = null
            });
        }

        return result;
    }
}
