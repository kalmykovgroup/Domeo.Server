namespace Clients.Abstractions.Routes;

/// <summary>
/// Route definitions for Clients.API service.
/// </summary>
public static class ClientsRoutes
{
    public const string ServiceName = "clients";

    /// <summary>
    /// Paths for controllers (relative, used in [Route] and [HttpGet])
    /// </summary>
    public static class Controller
    {
        public const string Base = "clients";
        public const string ById = "{id:guid}";
        public const string Restore = "{id:guid}/restore";
    }

    /// <summary>
    /// Paths for API Gateway (full paths for YARP)
    /// </summary>
    public static class Gateway
    {
        public const string Prefix = "/api/clients";
    }
}
