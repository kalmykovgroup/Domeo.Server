using Cabinets.API.Entities;
using Cabinets.API.Persistence;

namespace Cabinets.API.Services;

public sealed class CabinetsSeeder
{
    private readonly CabinetsDbContext _dbContext;
    private readonly ILogger<CabinetsSeeder> _logger;

    // Fixed GUIDs from Projects seeder
    private static readonly Guid Room1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid Edge1Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01");
    private static readonly Guid Zone1Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01");
    private static readonly Guid Zone2Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02");

    public CabinetsSeeder(CabinetsDbContext dbContext, ILogger<CabinetsSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding cabinets database...");

        // Base cabinets (floor level)
        var baseCabinets = new[]
        {
            CreateCabinetWithId(
                Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Room1Id, Edge1Id, Zone1Id,
                moduleTypeId: 1, // base_single_door
                "Тумба под мойку",
                "base",
                "shaker",
                0, 0, 0,
                600, 720, 560,
                15000m),
            CreateCabinetWithId(
                Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff1"),
                Room1Id, Edge1Id, Zone1Id,
                moduleTypeId: 2, // base_double_door
                "Тумба напольная 2 двери",
                "base",
                "shaker",
                600, 0, 0,
                800, 720, 560,
                18000m),
            CreateCabinetWithId(
                Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff2"),
                Room1Id, Edge1Id, Zone1Id,
                moduleTypeId: 3, // base_drawers
                "Тумба с ящиками",
                "base",
                "shaker",
                1400, 0, 0,
                600, 720, 560,
                22000m),
            CreateCabinetWithId(
                Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff3"),
                Room1Id, Edge1Id, Zone1Id,
                moduleTypeId: 4, // base_corner
                "Угловая тумба",
                "base",
                "shaker",
                2000, 0, 0,
                900, 720, 560,
                25000m)
        };

        _dbContext.Cabinets.AddRange(baseCabinets);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} base cabinets", baseCabinets.Length);

        // Wall cabinets (upper level)
        var wallCabinets = new[]
        {
            CreateCabinetWithId(
                Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff4"),
                Room1Id, Edge1Id, Zone2Id,
                moduleTypeId: 10, // wall_single_door
                "Шкаф навесной 1 дверь",
                "wall",
                "shaker",
                0, 1400, 0,
                600, 720, 320,
                12000m),
            CreateCabinetWithId(
                Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff5"),
                Room1Id, Edge1Id, Zone2Id,
                moduleTypeId: 11, // wall_double_door
                "Шкаф навесной 2 двери",
                "wall",
                "shaker",
                600, 1400, 0,
                800, 720, 320,
                15000m),
            CreateCabinetWithId(
                Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff6"),
                Room1Id, Edge1Id, Zone2Id,
                moduleTypeId: 12, // wall_glass_door
                "Шкаф навесной со стеклом",
                "wall",
                "glass",
                1400, 1400, 0,
                600, 720, 320,
                18000m)
        };

        _dbContext.Cabinets.AddRange(wallCabinets);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} wall cabinets", wallCabinets.Length);

        // Cabinet materials
        var materials = new[]
        {
            CabinetMaterial.Create(baseCabinets[0].Id, "body", Guid.Parse("11111111-0000-0000-0000-000000000001")),
            CabinetMaterial.Create(baseCabinets[0].Id, "facade", Guid.Parse("11111111-0000-0000-0000-000000000002")),
            CabinetMaterial.Create(baseCabinets[1].Id, "body", Guid.Parse("11111111-0000-0000-0000-000000000001")),
            CabinetMaterial.Create(baseCabinets[1].Id, "facade", Guid.Parse("11111111-0000-0000-0000-000000000002"))
        };

        _dbContext.CabinetMaterials.AddRange(materials);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} cabinet materials", materials.Length);

        _logger.LogInformation("Cabinets database seeding completed");
    }

    private static Cabinet CreateCabinetWithId(
        Guid id,
        Guid roomId,
        Guid? edgeId,
        Guid? zoneId,
        int? moduleTypeId,
        string name,
        string placementType,
        string? facadeType,
        double positionX,
        double positionY,
        double rotation,
        double width,
        double height,
        double depth,
        decimal? calculatedPrice)
    {
        var cabinet = Cabinet.Create(roomId, placementType, positionX, positionY, width, height, depth, name);
        typeof(Cabinet).GetProperty("Id")!.SetValue(cabinet, id);
        cabinet.SetEdge(edgeId);
        cabinet.SetZone(zoneId);
        cabinet.SetModuleType(moduleTypeId);
        cabinet.SetFacadeType(facadeType);
        cabinet.UpdatePosition(positionX, positionY, rotation);
        cabinet.SetCalculatedPrice(calculatedPrice);
        return cabinet;
    }
}
