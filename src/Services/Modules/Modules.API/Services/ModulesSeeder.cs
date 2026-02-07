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
            Component.Create("Ножка", new GlbParams("/uploads/glb/leg.glb", 1.0), ["leg", "glb"]),
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
        var leg = components.First(c => c.Name == "Ножка");

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
            var isStraightCorner = def.type.Contains("straight");
            var isCornerLLayout = isLShaped || isStraightCorner;
            var isRightOrientation = def.type.Contains("left");

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

            if (isCornerLLayout)
            {
                double armDepthVal;
                if (isStraightCorner)
                    armDepthVal = def.d - 30;
                else
                    armDepthVal = def.cat.StartsWith("wall") ? 304.0 : 544.0;

                parameters["armDepth"] = armDepthVal;
                parameters["backXOffset"] = 0;
                parameters["backZOffset"] = 0;
                parameters["shelf2Y"] = Math.Round(def.h * 2.0 / 3.0);
                paramConstraints["shelf2Y"] = new(32, def.h - 48, null);

                if (isStraightCorner)
                {
                    parameters["frontWidth"] = armDepthVal;
                    paramConstraints["frontWidth"] = new(100, def.d - 16, null);
                }
            }
            else
            {
                parameters["backOffset"] = 0;
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
                    shape: Rectangle("D", "H")));

                // #1 Right wall (reduced depth)
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "W", y: "0", z: "backOffset",
                    rotationY: -90,
                    sortOrder: 1,
                    shape: Rectangle("W-3*t-cutSize", "H")));

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
                    x: "t+shelfSideGap", y: "shelfY+t", z: "backOffset + shelfFrontInset",
                    rotationX: 90,
                    sortOrder: 5,
                    shape: Rectangle("W-2*t-2*shelfSideGap", "D-shelfRearInset-shelfFrontInset")));
            }
            else if (isCornerLLayout)
            {
                var isWall = def.cat.StartsWith("wall");
                var isRight = isRightOrientation;
                var s = 0;

                if (isRight)
                {
                    // === RIGHT mirror layout ===

                    // Right side back panel (wall only)
                    if (isWall)
                    {
                        allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                            x: "W", y: "2", z: "2",
                            rotationY: -90,
                            sortOrder: s++,
                            shape: BackPanelWithNotches("backZOffset + D - 4", "H - 4"),
                            provides: new Dictionary<string, string> { ["backXOffset"] = "tBack" }));
                    }

                    // Left wall (short arm)
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: "t", y: "0", z: "backZOffset",
                        rotationY: -90,
                        sortOrder: s++,
                        shape: Rectangle("armDepth", "H")));

                    // Top (L-shape right)
                    var rightFrontDepth = isStraightCorner ? "frontWidth - 2" : "armDepth - 2";
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: "t", y: "H", z: "backZOffset",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagonRight("W - t - backXOffset", "D - t", "armDepth - 2", rightFrontDepth)));

                    // Bottom (L-shape right)
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: "t", y: "t", z: "backZOffset",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagonRight("W - t - backXOffset", "D - t", "armDepth - 2", rightFrontDepth)));

                    // Rear back panel (wall only)
                    if (isWall)
                    {
                        allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                            x: "2", y: "2", z: "0",
                            sortOrder: s++,
                            shape: BackPanelWithNotches("W - backXOffset - 4", "H - 4"),
                            provides: new Dictionary<string, string> { ["backZOffset"] = "tBack" }));
                    }

                    // Shelf 1
                    var rightShelfFrontDepth = isStraightCorner ? "frontWidth - 2 - 2*shelfSideGap" : "armDepth - 2 - 2*shelfSideGap";
                    allParts.Add(AssemblyPart.Create(assembly.Id, shelfL.Id,
                        x: "t + shelfSideGap", y: "shelfY+t", z: "backZOffset + shelfSideGap",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagonRight(
                            "W - t - 2*shelfSideGap - backXOffset",
                            "D - t - 2*shelfSideGap",
                            "armDepth - 2 - 2*shelfSideGap",
                            rightShelfFrontDepth)));

                    // Shelf 2
                    allParts.Add(AssemblyPart.Create(assembly.Id, shelfL.Id,
                        x: "t + shelfSideGap", y: "shelf2Y+t", z: "backZOffset + shelfSideGap",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagonRight(
                            "W - t - 2*shelfSideGap - backXOffset",
                            "D - t - 2*shelfSideGap",
                            "armDepth - 2 - 2*shelfSideGap",
                            rightShelfFrontDepth)));

                    // Front side wall (right side)
                    var rightFrontWallX = isStraightCorner ? "W - frontWidth - backXOffset" : "W - armDepth - backXOffset";
                    var rightFrontWallW = isStraightCorner ? "frontWidth" : "armDepth";
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: rightFrontWallX, y: "0", z: "backZOffset + D - t",
                        sortOrder: s++,
                        shape: Rectangle(rightFrontWallW, "H")));

                    // Legs for base only
                    if (!isWall)
                    {
                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "t + 10", y: "0", z: "backZOffset + D - 10",
                            sortOrder: s++));

                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "W - backXOffset - 10", y: "0", z: "backZOffset + D - 10",
                            sortOrder: s++));

                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "t + 10", y: "0", z: "backZOffset + 10",
                            sortOrder: s++));

                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "W - backXOffset - 10", y: "0", z: "backZOffset + D - armDepth + 10",
                            sortOrder: s++));
                    }
                }
                else
                {
                    // === LEFT layout (L-shaped + straight-left) ===

                    // Left side back panel (wall only)
                    if (isWall)
                    {
                        allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                            x: "tBack", y: "2", z: "2",
                            rotationY: -90,
                            sortOrder: s++,
                            shape: BackPanelWithNotches("backZOffset + D - 4", "H - 4"),
                            provides: new Dictionary<string, string> { ["backXOffset"] = "tBack" }));
                    }

                    // Right wall
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: "backXOffset + W", y: "0", z: "backZOffset",
                        rotationY: -90,
                        sortOrder: s++,
                        shape: Rectangle("armDepth", "H")));

                    // Top (L-shape)
                    var leftFrontDepth = isStraightCorner ? "frontWidth - 2" : "armDepth - 2";
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: "backXOffset", y: "H", z: "backZOffset",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagon("W - t", "D - t", "armDepth - 2", leftFrontDepth)));

                    // Bottom (L-shape)
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: "backXOffset", y: "t", z: "backZOffset",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagon("W - t", "D - t", "armDepth - 2", leftFrontDepth)));

                    // Rear back panel (wall only)
                    if (isWall)
                    {
                        allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                            x: "2", y: "2", z: "0",
                            sortOrder: s++,
                            shape: BackPanelWithNotches("backXOffset + W - 4", "H - 4"),
                            provides: new Dictionary<string, string> { ["backZOffset"] = "tBack" }));
                    }

                    // Shelf 1
                    var leftShelfFrontDepth = isStraightCorner ? "frontWidth - 2 - 2*shelfSideGap" : "armDepth - 2 - 2*shelfSideGap";
                    allParts.Add(AssemblyPart.Create(assembly.Id, shelfL.Id,
                        x: "backXOffset + shelfSideGap", y: "shelfY+t", z: "backZOffset + shelfSideGap",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagon(
                            "W - t - 2*shelfSideGap",
                            "D - t - 2*shelfSideGap",
                            "armDepth - 2 - 2*shelfSideGap",
                            leftShelfFrontDepth)));

                    // Shelf 2
                    allParts.Add(AssemblyPart.Create(assembly.Id, shelfL.Id,
                        x: "backXOffset + shelfSideGap", y: "shelf2Y+t", z: "backZOffset + shelfSideGap",
                        rotationX: 90,
                        sortOrder: s++,
                        shape: LShapeHexagon(
                            "W - t - 2*shelfSideGap",
                            "D - t - 2*shelfSideGap",
                            "armDepth - 2 - 2*shelfSideGap",
                            leftShelfFrontDepth)));

                    // Front side wall
                    var leftFrontWallW = isStraightCorner ? "frontWidth" : "armDepth";
                    allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                        x: "backXOffset", y: "0", z: "backZOffset + D - t",
                        sortOrder: s++,
                        shape: Rectangle(leftFrontWallW, "H")));

                    // Legs for base only
                    if (!isWall)
                    {
                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "backXOffset + 10", y: "0", z: "backZOffset + D - 10",
                            sortOrder: s++));

                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "backXOffset + W - t - 10", y: "0", z: "backZOffset + D - 10",
                            sortOrder: s++));

                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "backXOffset + 10", y: "0", z: "backZOffset + D - armDepth + 10",
                            sortOrder: s++));

                        allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id,
                            x: "backXOffset + W - t - 10", y: "0", z: "backZOffset + 10",
                            sortOrder: s++));
                    }
                }
            }
            else
            {
                // Standard assemblies (straight, mezzanine, single/double door)

                // #0 Left wall
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "0", z: "backOffset",
                    rotationY: -90,
                    sortOrder: 0,
                    shape: Rectangle("D", "H")));

                // #1 Right wall
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "W", y: "0", z: "backOffset",
                    rotationY: -90,
                    sortOrder: 1,
                    shape: Rectangle("D", "H")));

                // #2 Top
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "H", z: "backOffset",
                    rotationX: 90,
                    sortOrder: 2,
                    shape: Rectangle("W - 2*t", "D")));

                // #3 Bottom
                allParts.Add(AssemblyPart.Create(assembly.Id, wall.Id,
                    x: "t", y: "t", z: "backOffset",
                    rotationX: 90,
                    sortOrder: 3,
                    shape: Rectangle("W - 2*t", "D")));

                // #4 Back panel (overlay on base)
                allParts.Add(AssemblyPart.Create(assembly.Id, back.Id,
                    x: "2", y: "2", z: "0",
                    sortOrder: 4,
                    shape: BackPanelWithNotches("W - 4", "H - 4"),
                    provides: new Dictionary<string, string> { ["backOffset"] = "tBack" }));

                // #5 Shelf
                allParts.Add(AssemblyPart.Create(assembly.Id, shelf.Id,
                    x: "t+shelfSideGap", y: "shelfY+t", z: "backOffset + shelfFrontInset",
                    rotationX: 90,
                    sortOrder: 5,
                    shape: Rectangle("W-2*t-2*shelfSideGap", "D-shelfRearInset-shelfFrontInset")));
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

    private static List<ShapeSegment> LShapeHexagon(string innerW, string innerD, string armDepth, string frontDepth) =>
    [
        new LineSegment("0", "0"),
        new LineSegment(innerW, "0"),
        new LineSegment(innerW, armDepth),
        new LineSegment(frontDepth, armDepth),
        new LineSegment(frontDepth, innerD),
        new LineSegment("0", innerD)
    ];

    private static List<ShapeSegment> LShapeHexagonRight(string innerW, string innerD, string armDepth, string frontDepth) =>
    [
        new LineSegment("0", "0"),
        new LineSegment(innerW, "0"),
        new LineSegment(innerW, innerD),
        new LineSegment($"{innerW} - {frontDepth}", innerD),
        new LineSegment($"{innerW} - {frontDepth}", armDepth),
        new LineSegment("0", armDepth)
    ];
}
