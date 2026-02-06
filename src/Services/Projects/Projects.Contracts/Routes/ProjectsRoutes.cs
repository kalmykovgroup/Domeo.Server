namespace Projects.Contracts.Routes;

public static class ProjectsRoutes
{
    public const string ServiceName = "projects";

    public static class Controller
    {
        // Base paths for controllers
        public const string Projects = "projects";
        public const string Rooms = "projects/{projectId:guid}/rooms";
        public const string RoomVertices = "rooms/{roomId:guid}/vertices";
        public const string RoomEdges = "rooms/{roomId:guid}/edges";
        public const string Zones = "edges/{edgeId:guid}/zones";
        public const string Cabinets = "cabinets";
        public const string CabinetParts = "cabinet-parts";

        // Relative paths for methods - Projects
        public const string ById = "{id:guid}";
        public const string Status = "{id:guid}/status";
        public const string Questionnaire = "{id:guid}/questionnaire";

        // Relative paths for Rooms
        public const string RoomById = "{roomId:guid}";

        // Relative paths for Vertices
        public const string VertexById = "{vertexId:guid}";

        // Relative paths for Edges
        public const string EdgeById = "{edgeId:guid}";

        // Relative paths for Zones
        public const string ZoneById = "{zoneId:guid}";

        // Relative paths for Cabinets
        public const string CabinetById = "{id:guid}";
        public const string CabinetsByRoom = "room/{roomId:guid}";

        // Relative paths for Cabinet Parts
        public const string CabinetPartById = "{id:guid}";
        public const string CabinetPartsByCabinet = "cabinet/{cabinetId:guid}";
        public const string CreateCabinetPart = "{cabinetId:guid}/parts";
    }

    public static class Gateway
    {
        public const string ProjectsPrefix = "/api/projects";
        public const string RoomsPrefix = "/api/rooms";
        public const string EdgesPrefix = "/api/edges";
        public const string CabinetsPrefix = "/api/cabinets";
        public const string CabinetPartsPrefix = "/api/cabinet-parts";
    }
}
