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
            Component.Create("Стенка", new PanelParams(16), ["panel", "wall"], color: "#D4A574"),
            Component.Create("Задняя стенка", new PanelParams(4), ["panel", "back"], color: "#E8D5B7"),
            Component.Create("Полка", new PanelParams(16), ["panel", "shelf"], color: "#B8956A"),
            Component.Create("Полка г-образная", new PanelParams(16), ["panel", "shelf", "l-shaped"], color: "#B8956A"),
            Component.Create("Ручка", new GlbParams("/uploads/glb/3DModelOfACabinetHandle.glb", 1.0), ["handle", "glb"]),
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
        var shelfL = components.First(c => c.Name == "Полка г-образная");

        var assemblyDefs = new (string cat, string type, string name,
            double w, double h, double d,
            double wMin, double wMax, double hMin, double hMax, double dMin, double dMax)[]
        {
            ("wall_corner", "wall-corner-straight-left", "Угловой прямой левый верхний", 600, 720, 320, 300, 900, 550, 920, 280, 350),
            ("wall_corner", "wall-corner-straight-right", "Угловой прямой правый верхний", 600, 720, 320, 300, 900, 550, 920, 280, 350),
            ("wall_corner", "wall-corner-l-shaped", "Угловой г-образный верхний", 600, 720, 600, 300, 1200, 550, 920, 550, 650),
            ("wall_corner", "wall-corner-diagonal", "Угловой диагональный верхний", 600, 720, 600, 550, 650, 550, 920, 550, 650),

            ("base_corner", "base-corner-straight-left", "Угловой прямой левый нижний", 900, 720, 560, 300, 1200, 720, 720, 500, 600),
            ("base_corner", "base-corner-straight-right", "Угловой прямой правый нижний", 900, 720, 560, 300, 1200, 720, 720, 500, 600),
            ("base_corner", "base-corner-l-shaped", "Угловой г-образный нижний", 600, 720, 900, 300, 1200, 720, 720, 800, 1000),
            ("base_corner", "base-corner-diagonal", "Угловой диагональный нижний", 900, 720, 900, 800, 1000, 720, 720, 800, 1000),

            ("mezzanine", "mezzanine", "Антресоль", 800, 360, 320, 300, 1200, 250, 550, 280, 350),

            ("wall_double_door", "wall-double-door", "Верхняя база двудверный", 800, 720, 320, 600, 1200, 550, 920, 280, 350),
            ("wall_single_door", "wall-single-door", "Верхняя база однодверный", 450, 720, 320, 300, 600, 550, 920, 280, 350),

            ("base_double_door", "base-double-door", "Нижняя база двудверный", 800, 720, 560, 600, 1200, 720, 720, 500, 600),
            ("base_single_door", "base-single-door", "Нижняя база однодверный", 450, 720, 560, 300, 600, 720, 720, 500, 600),
        };

        var assemblies = new List<Assembly>();
        var allParts = new List<AssemblyPart>();

        foreach (var def in assemblyDefs)
        {
            var isDiagonal = def.type.Contains("diagonal");
            var isLShaped = def.type.Contains("l-shaped");

            var parameters = new Dictionary<string, double>
            {
                ["W"] = def.w,
                ["H"] = def.h,
                ["D"] = def.d,
                ["t"] = 16,
                ["tBack"] = 4,
                ["facadeGap"] = 2,
                ["shelfSideGap"] = 2,
                ["shelfRearInset"] = 20,
                ["shelfFrontInset"] = 10,
                ["shelfY"] = Math.Round(def.h / 3.0)
            };

            var paramConstraints = new Dictionary<string, ParamConstraint>
            {
                ["W"] = new(def.wMin, def.wMax, null),
                ["H"] = new(def.hMin, def.hMax, null),
                ["D"] = new(def.dMin, def.dMax, null),
                ["shelfY"] = new(32, def.h - 48, null)
            };

            if (isLShaped)
            {
                var armDepthVal = def.cat.StartsWith("wall") ? 304.0 : 544.0;
                parameters["armDepth"] = armDepthVal;
            }

            if (isDiagonal)
            {
                parameters["cutSize"] = 300;
            }

            var assembly = Assembly.Create(def.cat, def.type, def.name, parameters, paramConstraints);
            assemblies.Add(assembly);

            if (isDiagonal)
            {
                // #0 Left wall
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "0", z: "backOffset",
                    rotationY: -90,
                    sortOrder: 0,
                    shape: Rectangle("D - backOffset", "H")));

                // #1 Right wall (reduced depth)
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "W", y: "0", z: "backOffset",
                    rotationY: -90,
                    sortOrder: 1,
                    shape: Rectangle("W-3*t-cutSize - backOffset", "H")));

                // #2 Top
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "H", z: "backOffset",
                    rotationX: 90,
                    sortOrder: 2,
                    shape: DiagonalPentagon("W-2*t", "W-2*t-cutSize")));

                // #3 Bottom
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "t", z: "backOffset",
                    rotationX: 90,
                    sortOrder: 3,
                    shape: DiagonalPentagon("W-2*t", "W-2*t-cutSize")));

                // #4 Back panel (overlay on base)
                allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                    x: "2", y: "2", z: "0",
                    sortOrder: 4,
                    shape: BackPanelWithNotches("W - 4", "H - 4"),
                    provides: new Dictionary<string, string> { ["backOffset"] = "tBack" }));

                // #5 Shelf
                allParts.Add(AssemblyPart.Create(assembly.Id, shelf.Id,
                    x: "t+shelfSideGap", y: "shelfY+t", z: "shelfFrontInset",
                    rotationX: 90,
                    sortOrder: 5,
                    shape: Rectangle("W-2*t-2*shelfSideGap", "D-2*backOffset-shelfRearInset-shelfFrontInset")));
            }
            else if (isLShaped)
            {
                // L-shaped: левая сторона у стены (нет левой боковой),
                // есть передняя боковая (x=0,z=D) и правая боковая (x=W)

                // #0 Left back panel (overlay on left side)
                allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                    x: "2", y: "2", z: "0",
                    rotationY: -90,
                    sortOrder: 0,
                    shape: BackPanelWithNotches("D - 4", "H - 4"),
                    provides: new Dictionary<string, string> { ["backXOffset"] = "tBack" }));

                // #1 Right wall (armDepth deep)
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "W", y: "0", z: "backZOffset",
                    rotationY: -90,
                    sortOrder: 1,
                    shape: Rectangle("armDepth", "H")));

                // #2 Top (L-shape)
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "0", y: "H", z: "backZOffset",
                    rotationX: 90,
                    sortOrder: 2,
                    shape: LShapeHexagon("W - t", "D - t - backZOffset", "armDepth - 2")));

                // #3 Bottom (L-shape)
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "0", y: "t", z: "backZOffset",
                    rotationX: 90,
                    sortOrder: 3,
                    shape: LShapeHexagon("W - t", "D - t - backZOffset", "armDepth - 2")));

                // #4 Right back panel (overlay on rear side)
                allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                    x: "2", y: "2", z: "2",
                    sortOrder: 4,
                    shape: BackPanelWithNotches("W - 4", "H - 4"),
                    provides: new Dictionary<string, string> { ["backZOffset"] = "tBack" }));

                // #5 Shelf (L-shape)
                allParts.Add(AssemblyPart.Create(assembly.Id, shelfL.Id,
                    x: "shelfSideGap", y: "shelfY+t", z: "shelfFrontInset",
                    rotationX: 90,
                    sortOrder: 5,
                    shape: LShapeHexagon(
                        "W - t - 2*shelfSideGap",
                        "D - t - backZOffset - shelfRearInset - shelfFrontInset",
                        "armDepth - 2")));

                // #6 Front side wall
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "0", y: "0", z: "D - t",
                    sortOrder: 6,
                    shape: Rectangle("armDepth", "H")));
            }
            else
            {
                // Standard assemblies (straight, mezzanine, single/double door)

                // #0 Left wall
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "0", z: "backOffset",
                    rotationY: -90,
                    sortOrder: 0,
                    shape: Rectangle("D - backOffset", "H")));

                // #1 Right wall
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "W", y: "0", z: "backOffset",
                    rotationY: -90,
                    sortOrder: 1,
                    shape: Rectangle("D - backOffset", "H")));

                // #2 Top
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "H", z: "backOffset",
                    rotationX: 90,
                    sortOrder: 2,
                    shape: Rectangle("W - 2*t", "D - 2*backOffset")));

                // #3 Bottom
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "t", z: "backOffset",
                    rotationX: 90,
                    sortOrder: 3,
                    shape: Rectangle("W - 2*t", "D - 2*backOffset")));

                // #4 Back panel (overlay on base)
                allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                    x: "2", y: "2", z: "0",
                    sortOrder: 4,
                    shape: BackPanelWithNotches("W - 4", "H - 4"),
                    provides: new Dictionary<string, string> { ["backOffset"] = "tBack" }));

                // #5 Shelf
                allParts.Add(AssemblyPart.Create(assembly.Id, shelf.Id,
                    x: "t+shelfSideGap", y: "shelfY+t", z: "shelfFrontInset",
                    rotationX: 90,
                    sortOrder: 5,
                    shape: Rectangle("W-2*t-2*shelfSideGap", "D-2*backOffset-shelfRearInset-shelfFrontInset")));
            }
        }

        _dbContext.Assemblies.AddRange(assemblies);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} assemblies", assemblies.Count);

        _dbContext.AssemblyParts.AddRange(allParts);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} assembly parts", allParts.Count);
    }

    private static List<ShapeSegment> Rectangle(string w, string h) =>
    [
        new LineSegment("0", "0"),
        new LineSegment(w, "0"),
        new LineSegment(w, h),
        new LineSegment("0", h)
    ];

    private static List<ShapeSegment> BackPanelWithNotches(string w, string h) =>
    [
        new LineSegment("0", "0"),
        new LineSegment(w, "0"),
        new LineSegment(w, $"{h} - 50"),
        new LineSegment($"{w} - 25", $"{h} - 50"),
        new LineSegment($"{w} - 25", h),
        new LineSegment("25", h),
        new LineSegment("25", $"{h} - 50"),
        new LineSegment("0", $"{h} - 50")
    ];

    private static List<ShapeSegment> DiagonalPentagon(string size, string cut) =>
    [
        new LineSegment("0", "0"),
        new LineSegment(size, "0"),
        new LineSegment(size, cut),
        new LineSegment(cut, size),
        new LineSegment("0", size)
    ];

    private static List<ShapeSegment> LShapeHexagon(string innerW, string innerD, string armDepth) =>
    [
        new LineSegment("0", "0"),
        new LineSegment(innerW, "0"),
        new LineSegment(innerW, armDepth),
        new LineSegment(armDepth, armDepth),
        new LineSegment(armDepth, innerD),
        new LineSegment("0", innerD)
    ];
}
