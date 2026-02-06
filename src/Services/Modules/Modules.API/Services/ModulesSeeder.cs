using Microsoft.EntityFrameworkCore;
using Modules.Domain.Entities;
using Modules.Domain.Entities.Shared;
using Modules.Infrastructure.Persistence;

namespace Modules.API.Services;

public sealed class ModulesSeeder
{
    private readonly ModulesDbContext _dbContext;
    private readonly ILogger<ModulesSeeder> _logger;

    public ModulesSeeder(ModulesDbContext dbContext, ILogger<ModulesSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SeedCategoriesAsync(cancellationToken);
            await SeedComponentsAsync(cancellationToken);
            await SeedAssembliesAsync(cancellationToken);

            _logger.LogInformation("Modules seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during modules seeding");
            throw;
        }
    }

    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        if (await _dbContext.ModuleCategories.AnyAsync(ct)) return;

        var categories = new List<ModuleCategory>
        {
            ModuleCategory.Create("base", "Нижние шкафы", description: "Шкафы для нижней зоны кухни", orderIndex: 1),
            ModuleCategory.Create("base_single_door", "Однодверный", "base", "Шкаф с одной распашной дверью", 1),
            ModuleCategory.Create("base_double_door", "Двудверный", "base", "Шкаф с двумя распашными дверями", 2),
            ModuleCategory.Create("base_corner", "Угловой", "base", "Угловой нижний шкаф", 3),

            ModuleCategory.Create("wall", "Верхние шкафы", description: "Шкафы для верхней зоны кухни", orderIndex: 2),
            ModuleCategory.Create("wall_single_door", "Однодверный", "wall", "Шкаф с одной распашной дверью", 1),
            ModuleCategory.Create("wall_double_door", "Двудверный", "wall", "Шкаф с двумя распашными дверями", 2),
            ModuleCategory.Create("wall_corner", "Угловой", "wall", "Угловой верхний шкаф", 3),

            ModuleCategory.Create("mezzanine", "Антресоль", description: "Шкафы-антресоли под потолком", orderIndex: 3),
        };

        _dbContext.ModuleCategories.AddRange(categories);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} module categories", categories.Count);
    }

    private async Task SeedComponentsAsync(CancellationToken ct)
    {
        if (await _dbContext.Components.AnyAsync(ct)) return;

        var components = new List<Component>
        {
            Component.Create("Стенка", new PanelParams(16), ["panel", "wall"]),
            Component.Create("Задняя стенка", new PanelParams(4), ["panel", "back"]),
            Component.Create("Полка", new PanelParams(16), ["panel", "shelf"]),
        };

        _dbContext.Components.AddRange(components);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} components", components.Count);
    }

    private async Task SeedAssembliesAsync(CancellationToken ct)
    {
        if (await _dbContext.Assemblies.AnyAsync(ct)) return;

        var components = await _dbContext.Components.ToListAsync(ct);
        var wall = components.First(c => c.Name == "Стенка");
        var back = components.First(c => c.Name == "Задняя стенка");
        var shelf = components.First(c => c.Name == "Полка");

        var defaultConstruction = new Construction(16, 4, 18, 2, 0, 2, 20, 10);

        var t = defaultConstruction.PanelThickness;    // 16
        var sg = defaultConstruction.ShelfSideGap;      // 2
        var ri = defaultConstruction.ShelfRearInset;    // 20
        var fi = defaultConstruction.ShelfFrontInset;   // 10

        // Assembly definitions: (categoryId, type, name, w, h, d, wMin, wMax, hMin, hMax, dMin, dMax)
        var assemblyDefs = new (string cat, string type, string name,
            double w, double h, double d,
            double wMin, double wMax, double hMin, double hMax, double dMin, double dMax)[]
        {
            // Угловые верхние
            ("wall_corner", "wall-corner-straight-left", "Угловой прямой левый верхний", 600, 720, 320, 300, 900, 550, 920, 280, 350),
            ("wall_corner", "wall-corner-straight-right", "Угловой прямой правый верхний", 600, 720, 320, 300, 900, 550, 920, 280, 350),
            ("wall_corner", "wall-corner-l-shaped", "Угловой г-образный верхний", 600, 720, 600, 300, 1200, 550, 920, 550, 650),
            ("wall_corner", "wall-corner-diagonal", "Угловой диагональный верхний", 600, 720, 600, 550, 650, 550, 920, 550, 650),

            // Угловые нижние
            ("base_corner", "base-corner-straight-left", "Угловой прямой левый нижний", 900, 720, 560, 300, 1200, 720, 720, 500, 600),
            ("base_corner", "base-corner-straight-right", "Угловой прямой правый нижний", 900, 720, 560, 300, 1200, 720, 720, 500, 600),
            ("base_corner", "base-corner-l-shaped", "Угловой г-образный нижний", 600, 720, 900, 300, 1200, 720, 720, 800, 1000),
            ("base_corner", "base-corner-diagonal", "Угловой диагональный нижний", 900, 720, 900, 800, 1000, 720, 720, 800, 1000),

            // Антресоль
            ("mezzanine", "mezzanine", "Антресоль", 800, 360, 320, 300, 1200, 250, 550, 280, 350),

            // Верхние базы
            ("wall_double_door", "wall-double-door", "Верхняя база двудверный", 800, 720, 320, 600, 1200, 550, 920, 280, 350),
            ("wall_single_door", "wall-single-door", "Верхняя база однодверный", 450, 720, 320, 300, 600, 550, 920, 280, 350),

            // Нижние базы
            ("base_double_door", "base-double-door", "Нижняя база двудверный", 800, 720, 560, 600, 1200, 720, 720, 500, 600),
            ("base_single_door", "base-single-door", "Нижняя база однодверный", 450, 720, 560, 300, 600, 720, 720, 500, 600),
        };

        var assemblies = new List<Assembly>();
        var allParts = new List<AssemblyPart>();

        foreach (var def in assemblyDefs)
        {
            var assembly = Assembly.Create(
                def.cat, def.type, def.name,
                new Dimensions(def.w, def.d, def.h),
                new Constraints(def.wMin, def.wMax, def.hMin, def.hMax, def.dMin, def.dMax),
                defaultConstruction);

            assemblies.Add(assembly);

            // Left: полная высота, полная глубина, привязка к левому краю
            allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id, PartRole.Left,
                placement: new Placement(AnchorOrigin.Start, AnchorOrigin.Start, AnchorOrigin.Start, 0, 0, 0, 0, 0, 0),
                length: new DynamicSize(DimensionSource.ParentHeight, 0, null),
                width: new DynamicSize(DimensionSource.ParentDepth, 0, null),
                sortOrder: 0));

            // Right: полная высота, полная глубина, привязка к правому краю
            allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id, PartRole.Right,
                placement: new Placement(AnchorOrigin.End, AnchorOrigin.Start, AnchorOrigin.Start, 0, 0, 0, 0, 0, 0),
                length: new DynamicSize(DimensionSource.ParentHeight, 0, null),
                width: new DynamicSize(DimensionSource.ParentDepth, 0, null),
                sortOrder: 1));

            // Top: ширина минус две стенки, полная глубина, привязка к верху
            allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id, PartRole.Top,
                placement: new Placement(AnchorOrigin.Start, AnchorOrigin.End, AnchorOrigin.Start, t, 0, 0, 0, 0, 0),
                length: new DynamicSize(DimensionSource.ParentWidth, -2 * t, null),
                width: new DynamicSize(DimensionSource.ParentDepth, 0, null),
                sortOrder: 2));

            // Bottom: ширина минус две стенки, полная глубина, привязка к низу
            allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id, PartRole.Bottom,
                placement: new Placement(AnchorOrigin.Start, AnchorOrigin.Start, AnchorOrigin.Start, t, 0, 0, 0, 0, 0),
                length: new DynamicSize(DimensionSource.ParentWidth, -2 * t, null),
                width: new DynamicSize(DimensionSource.ParentDepth, 0, null),
                sortOrder: 3));

            // Back: ширина минус две стенки, полная высота, привязка к задней стенке
            allParts.Add(AssemblyPart.Create(assembly.Id, back.Id, PartRole.Back,
                placement: new Placement(AnchorOrigin.Start, AnchorOrigin.Start, AnchorOrigin.End, t, 0, 0, 0, 0, 0),
                length: new DynamicSize(DimensionSource.ParentWidth, -2 * t, null),
                width: new DynamicSize(DimensionSource.ParentHeight, 0, null),
                sortOrder: 4));

            // Shelf: ширина минус стенки и зазоры, глубина минус отступы спереди и сзади
            allParts.Add(AssemblyPart.Create(assembly.Id, shelf.Id, PartRole.Shelf,
                placement: new Placement(AnchorOrigin.Start, AnchorOrigin.Center, AnchorOrigin.Start, t + sg, 0, fi, 0, 0, 0),
                length: new DynamicSize(DimensionSource.ParentWidth, -2 * t - 2 * sg, null),
                width: new DynamicSize(DimensionSource.ParentDepth, -ri - fi, null),
                sortOrder: 5));
        }

        _dbContext.Assemblies.AddRange(assemblies);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} assemblies", assemblies.Count);

        _dbContext.AssemblyParts.AddRange(allParts);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} assembly parts", allParts.Count);
    }
}
