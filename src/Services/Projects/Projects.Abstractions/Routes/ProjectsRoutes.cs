namespace Projects.Abstractions.Routes;

/// <summary>
/// Route definitions for Projects.API service.
/// </summary>
public static class ProjectsRoutes
{
    public const string ServiceName = "projects";

    /// <summary>
    /// Paths for controllers (relative, used in [Route] and [HttpGet])
    /// </summary>
    public static class Controller
    {
        // Base paths for controllers
        public const string Projects = "projects";
        public const string Cabinets = "cabinets";
        public const string CabinetHardwareOverrides = "cabinet-hardware-overrides";

        // Relative paths for methods - Projects
        public const string ById = "{id:guid}";
        public const string Status = "{id:guid}/status";
        public const string Questionnaire = "{id:guid}/questionnaire";

        // Relative paths for Rooms (nested under projects)
        public const string Rooms = "/projects/{projectId:guid}/rooms";
        public const string RoomById = "{roomId:guid}";

        // Relative paths for Vertices (nested under rooms)
        public const string Vertices = "/rooms/{roomId:guid}/vertices";
        public const string VertexById = "{vertexId:guid}";

        // Relative paths for Edges (nested under rooms)
        public const string Edges = "/rooms/{roomId:guid}/edges";
        public const string EdgeById = "{edgeId:guid}";

        // Relative paths for Zones (nested under edges)
        public const string Zones = "/edges/{edgeId:guid}/zones";
        public const string ZoneById = "{zoneId:guid}";

        // Relative paths for Cabinets
        public const string CabinetById = "{id:guid}";
        public const string CabinetsByRoom = "room/{roomId:guid}";

        // Relative paths for Cabinet Hardware Overrides
        public const string CabinetHardwareOverridesNested = "/cabinets/{cabinetId:guid}/hardware-overrides";
        public const string HardwareOverrideById = "{id:guid}";
    }

    /// <summary>
    /// Paths for API Gateway (full paths for YARP)
    /// </summary>
    public static class Gateway
    {
        public const string ProjectsPrefix = "/api/projects";
        public const string RoomsPrefix = "/api/rooms";
        public const string EdgesPrefix = "/api/edges";
        public const string CabinetsPrefix = "/api/cabinets";
        public const string CabinetHardwareOverridesPrefix = "/api/cabinet-hardware-overrides";
    }
}
