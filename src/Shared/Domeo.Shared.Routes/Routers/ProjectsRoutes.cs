namespace Domeo.Shared.Routes.Routers;

/// <summary>
/// Route definitions for Projects.API service.
/// Internal paths: /projects/*, /rooms/*, /edges/*, /cabinets/*, /cabinet-hardware-overrides/*
/// </summary>
public static class ProjectsRoutes
{
    public const string ServiceName = "projects";

    // Internal paths - Projects
    public const string Projects = "/projects";
    public const string ProjectById = "/projects/{id:guid}";

    // Internal paths - Rooms (nested under projects)
    public const string Rooms = "/projects/{projectId:guid}/rooms";
    public const string RoomById = "/projects/{projectId:guid}/rooms/{roomId:guid}";

    // Internal paths - Vertices (nested under rooms)
    public const string Vertices = "/rooms/{roomId:guid}/vertices";
    public const string VertexById = "/rooms/{roomId:guid}/vertices/{vertexId:guid}";

    // Internal paths - Edges (nested under rooms)
    public const string Edges = "/rooms/{roomId:guid}/edges";
    public const string EdgeById = "/rooms/{roomId:guid}/edges/{edgeId:guid}";

    // Internal paths - Zones (nested under edges)
    public const string Zones = "/edges/{edgeId:guid}/zones";
    public const string ZoneById = "/edges/{edgeId:guid}/zones/{zoneId:guid}";

    // Internal paths - Cabinets
    public const string Cabinets = "/cabinets";
    public const string CabinetById = "/cabinets/{id:guid}";
    public const string CabinetsByRoom = "/cabinets/room/{roomId:guid}";

    // Internal paths - Cabinet Hardware Overrides
    public const string CabinetHardwareOverrides = "/cabinets/{cabinetId:guid}/hardware-overrides";
    public const string CabinetHardwareOverrideById = "/cabinet-hardware-overrides/{id:guid}";

    // Gateway paths (external API)
    public static class Gateway
    {
        public const string ProjectsPrefix = "/api/projects";
        public const string RoomsPrefix = "/api/rooms";
        public const string EdgesPrefix = "/api/edges";
        public const string CabinetsPrefix = "/api/cabinets";
        public const string CabinetHardwareOverridesPrefix = "/api/cabinet-hardware-overrides";
    }
}
