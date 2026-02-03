using Projects.API.Entities;
using Projects.API.Persistence;

namespace Projects.API.Services;

public sealed class ProjectsSeeder
{
    private readonly ProjectsDbContext _dbContext;
    private readonly ILogger<ProjectsSeeder> _logger;

    public ProjectsSeeder(ProjectsDbContext dbContext, ILogger<ProjectsSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding projects database...");

        // Note: We use fixed GUIDs that match the Users seeder for foreign keys
        // In real scenario these would be fetched from Users.API
        var designerUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var client1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var client2Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Project 1: Kitchen project with rooms
        var project1 = CreateProjectWithId(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Кухня Петрова",
            "kitchen",
            client1Id,
            designerUserId,
            "Современная кухня в стиле минимализм");
        project1.UpdateStatus(ProjectStatus.InProgress);

        _dbContext.Projects.Add(project1);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Room for project 1
        var room1 = CreateRoomWithId(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            project1.Id,
            "Кухня",
            2700,
            0);
        _dbContext.Rooms.Add(room1);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Vertices for room 1 (rectangular room 4x3 meters)
        var v1 = CreateVertexWithId(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc01"), room1.Id, 0, 0, 0);
        var v2 = CreateVertexWithId(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc02"), room1.Id, 4000, 0, 1);
        var v3 = CreateVertexWithId(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc03"), room1.Id, 4000, 3000, 2);
        var v4 = CreateVertexWithId(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc04"), room1.Id, 0, 3000, 3);

        _dbContext.RoomVertices.AddRange(v1, v2, v3, v4);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Edges for room 1
        var e1 = CreateEdgeWithId(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01"), room1.Id, v1.Id, v2.Id, 2700, false, false, 0);
        var e2 = CreateEdgeWithId(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02"), room1.Id, v2.Id, v3.Id, 2700, true, false, 1);
        var e3 = CreateEdgeWithId(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd03"), room1.Id, v3.Id, v4.Id, 2700, false, true, 2);
        var e4 = CreateEdgeWithId(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd04"), room1.Id, v4.Id, v1.Id, 2700, false, false, 3);

        _dbContext.RoomEdges.AddRange(e1, e2, e3, e4);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Zones on edge 1 (base cabinets zone)
        var zone1 = CreateZoneWithId(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01"), e1.Id, ZoneType.Base, 0, 4000, "Нижние шкафы");
        var zone2 = CreateZoneWithId(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02"), e1.Id, ZoneType.Wall, 0, 4000, "Верхние шкафы");

        _dbContext.Zones.AddRange(zone1, zone2);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded project 1 with room, vertices, edges, and zones");

        // Project 2: Simple draft project
        var project2 = CreateProjectWithId(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab"),
            "Кухня Сидоровой",
            "kitchen",
            client2Id,
            designerUserId,
            "Классическая кухня");

        _dbContext.Projects.Add(project2);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded project 2 (draft)");

        _logger.LogInformation("Projects database seeding completed");
    }

    private static Project CreateProjectWithId(Guid id, string name, string type, Guid clientId, Guid userId, string? notes)
    {
        var project = Project.Create(name, type, clientId, userId, notes);
        // Use reflection to set Id
        typeof(Project).GetProperty("Id")!.SetValue(project, id);
        return project;
    }

    private static Room CreateRoomWithId(Guid id, Guid projectId, string name, int ceilingHeight, int orderIndex)
    {
        var room = Room.Create(projectId, name, ceilingHeight, orderIndex);
        typeof(Room).GetProperty("Id")!.SetValue(room, id);
        return room;
    }

    private static RoomVertex CreateVertexWithId(Guid id, Guid roomId, double x, double y, int orderIndex)
    {
        var vertex = RoomVertex.Create(roomId, x, y, orderIndex);
        typeof(RoomVertex).GetProperty("Id")!.SetValue(vertex, id);
        return vertex;
    }

    private static RoomEdge CreateEdgeWithId(Guid id, Guid roomId, Guid startVertexId, Guid endVertexId, int wallHeight, bool hasWindow, bool hasDoor, int orderIndex)
    {
        var edge = RoomEdge.Create(roomId, startVertexId, endVertexId, wallHeight, hasWindow, hasDoor, orderIndex);
        typeof(RoomEdge).GetProperty("Id")!.SetValue(edge, id);
        return edge;
    }

    private static Zone CreateZoneWithId(Guid id, Guid edgeId, ZoneType type, double startX, double endX, string? name)
    {
        var zone = Zone.Create(edgeId, type, startX, endX, name);
        typeof(Zone).GetProperty("Id")!.SetValue(zone, id);
        return zone;
    }
}
