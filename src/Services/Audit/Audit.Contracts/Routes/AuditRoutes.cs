namespace Audit.Contracts.Routes;

public static class AuditRoutes
{
    public const string ServiceName = "audit";

    public static class Controller
    {
        public const string Logs = "audit/logs";
        public const string LoginSessions = "audit/login-sessions";
        public const string ApplicationLogs = "audit/application-logs";

        public const string EntityHistory = "entity/{entityType}/{entityId}";
        public const string ById = "{id:guid}";
        public const string Logout = "{id:guid}/logout";
        public const string UserSessions = "user/{userId:guid}";
        public const string Stats = "stats";
    }

    public static class Gateway
    {
        public const string Prefix = "/api/audit";
    }
}
