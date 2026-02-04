namespace Audit.Abstractions.Routes;

/// <summary>
/// Route definitions for Audit.API service.
/// </summary>
public static class AuditRoutes
{
    public const string ServiceName = "audit";

    /// <summary>
    /// Paths for controllers (relative, used in [Route] and [HttpGet])
    /// </summary>
    public static class Controller
    {
        // Base paths for controllers
        public const string Logs = "audit/logs";
        public const string LoginSessions = "audit/login-sessions";
        public const string ApplicationLogs = "audit/application-logs";

        // Relative paths for methods
        public const string EntityHistory = "entity/{entityType}/{entityId}";
        public const string ById = "{id:guid}";
        public const string Logout = "{id:guid}/logout";
        public const string UserSessions = "user/{userId:guid}";
        public const string Stats = "stats";
    }

    /// <summary>
    /// Paths for API Gateway (full paths for YARP)
    /// </summary>
    public static class Gateway
    {
        public const string Prefix = "/api/audit";
    }
}
