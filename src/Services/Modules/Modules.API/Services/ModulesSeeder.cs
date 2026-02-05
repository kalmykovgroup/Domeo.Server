using Microsoft.EntityFrameworkCore;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Entities.Shared;
using Modules.API.Infrastructure.Persistence;

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
            ModuleCategory.Create("base_triple_door", "Трёхдверный", "base", "Шкаф с тремя распашными дверями", 3),
            ModuleCategory.Create("base_with_drawers", "С ящиками", "base", "Шкаф с выдвижными ящиками", 4),
            ModuleCategory.Create("base_corner", "Угловой", "base", "Угловой шкаф (L-образный, диагональный, глухой)", 5),
            ModuleCategory.Create("base_for_appliance", "Под технику", "base", "Шкаф под встроенную технику", 6),

            ModuleCategory.Create("wall", "Верхние шкафы", description: "Шкафы для верхней зоны кухни", orderIndex: 2),
            ModuleCategory.Create("wall_single_door", "Однодверный", "wall", "Шкаф с одной распашной дверью", 1),
            ModuleCategory.Create("wall_double_door", "Двудверный", "wall", "Шкаф с двумя распашными дверями", 2),
            ModuleCategory.Create("wall_open", "Открытый", "wall", "Шкаф без дверей (открытые полки)", 3),
            ModuleCategory.Create("wall_corner", "Угловой", "wall", "Угловой верхний шкаф", 4),

            ModuleCategory.Create("mezzanine", "Антресоль", description: "Шкафы-антресоли под потолком", orderIndex: 3),
            ModuleCategory.Create("mezzanine_single_door", "Однодверный", "mezzanine", "Антресоль с одной дверью", 1),
            ModuleCategory.Create("mezzanine_double_door", "Двудверный", "mezzanine", "Антресоль с двумя дверями", 2),

            ModuleCategory.Create("tall", "Высокие шкафы", description: "Высокие шкафы-пеналы", orderIndex: 4),
            ModuleCategory.Create("tall_single_door", "Однодверный", "tall", "Пенал с одной дверью", 1),
            ModuleCategory.Create("tall_double_door", "Двудверный", "tall", "Пенал с двумя дверями", 2),
            ModuleCategory.Create("tall_for_appliance", "Под духовку", "tall", "Пенал под встроенную духовку", 3),
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
            Component.Create("Панель ДСП 16мм", new PanelParams(16), ["panel", "dsp"]),
            Component.Create("Панель задняя ДВП 4мм", new PanelParams(4), ["panel", "dvp", "back"]),
            Component.Create("Панель фасадная МДФ 18мм", new PanelParams(18), ["panel", "mdf", "facade"]),
            Component.Create("Петля Blum Clip Top", new GlbParams("/models/hardware/blum-clip-top.glb", 1), ["hinge", "blum"]),
            Component.Create("Ручка IKEA 128мм", new GlbParams("/models/hardware/ikea-handle-128.glb", 1), ["handle", "ikea"]),
            Component.Create("Ножка регулируемая", new GlbParams("/models/hardware/adjustable-leg.glb", 1), ["leg"]),
            Component.Create("Кронштейн подвесной", new GlbParams("/models/hardware/wall-mounting-bracket.glb", 1), ["mounting", "bracket"]),
            Component.Create("Глухая планка", new PanelParams(16), ["panel", "divider"]),
        };

        _dbContext.Components.AddRange(components);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} components", components.Count);
    }

    private async Task SeedAssembliesAsync(CancellationToken ct)
    {
        if (await _dbContext.Assemblies.AnyAsync(ct)) return;

        var components = await _dbContext.Components.ToListAsync(ct);
        var panelDsp = components.First(c => c.Name == "Панель ДСП 16мм");
        var panelBack = components.First(c => c.Name == "Панель задняя ДВП 4мм");
        var panelFacade = components.First(c => c.Name == "Панель фасадная МДФ 18мм");
        var hinge = components.First(c => c.Name == "Петля Blum Clip Top");
        var handle = components.First(c => c.Name == "Ручка IKEA 128мм");
        var leg = components.First(c => c.Name == "Ножка регулируемая");
        var bracket = components.First(c => c.Name == "Кронштейн подвесной");
        var divider = components.First(c => c.Name == "Глухая планка");

        var defaultPlacement = new Placement(
            AnchorOrigin.Start, AnchorOrigin.Start, AnchorOrigin.Start,
            0, 0, 0, 0, 0, 0);

        var defaultConstruction = new Construction(16, 4, 18, 2, 0, 2, 20, 10);

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

            var sortOrder = 0;

            // Left panel
            allParts.Add(AssemblyPart.Create(assembly.Id, panelDsp.Id, PartRole.Left, defaultPlacement, sortOrder: sortOrder++));
            // Right panel
            allParts.Add(AssemblyPart.Create(assembly.Id, panelDsp.Id, PartRole.Right, defaultPlacement, sortOrder: sortOrder++));
            // Top panel
            allParts.Add(AssemblyPart.Create(assembly.Id, panelDsp.Id, PartRole.Top, defaultPlacement, sortOrder: sortOrder++));
            // Bottom panel
            allParts.Add(AssemblyPart.Create(assembly.Id, panelDsp.Id, PartRole.Bottom, defaultPlacement, sortOrder: sortOrder++));
            // Back panel
            allParts.Add(AssemblyPart.Create(assembly.Id, panelBack.Id, PartRole.Back, defaultPlacement, sortOrder: sortOrder++));

            // Facade
            allParts.Add(AssemblyPart.Create(assembly.Id, panelFacade.Id, PartRole.Facade, defaultPlacement, sortOrder: sortOrder++));

            // Hinges (2 per door)
            allParts.Add(AssemblyPart.Create(assembly.Id, hinge.Id, PartRole.Hinge, defaultPlacement, quantity: 2, sortOrder: sortOrder++));

            // Handle
            allParts.Add(AssemblyPart.Create(assembly.Id, handle.Id, PartRole.Handle, defaultPlacement, sortOrder: sortOrder++));

            // Legs for base modules
            if (def.cat.StartsWith("base"))
            {
                allParts.Add(AssemblyPart.Create(assembly.Id, leg.Id, PartRole.Leg, defaultPlacement, quantity: 4, sortOrder: sortOrder++));
            }

            // Mounting brackets for wall/mezzanine
            if (def.cat.StartsWith("wall") || def.cat.StartsWith("mezzanine"))
            {
                allParts.Add(AssemblyPart.Create(assembly.Id, bracket.Id, PartRole.Handle, defaultPlacement, quantity: 2, sortOrder: sortOrder++));
            }

            // Dividers for straight corner assemblies
            if (def.type.Contains("straight"))
            {
                allParts.Add(AssemblyPart.Create(assembly.Id, divider.Id, PartRole.Divider, defaultPlacement, sortOrder: sortOrder++));
            }

            // Shelf for most assemblies
            allParts.Add(AssemblyPart.Create(assembly.Id, panelDsp.Id, PartRole.Shelf, defaultPlacement, sortOrder: sortOrder));
        }

        _dbContext.Assemblies.AddRange(assemblies);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} assemblies", assemblies.Count);

        _dbContext.AssemblyParts.AddRange(allParts);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} assembly parts", allParts.Count);
    }
}
