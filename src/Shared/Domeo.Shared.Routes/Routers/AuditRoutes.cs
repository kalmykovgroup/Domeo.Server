namespace Domeo.Shared.Routes.Routers;

/// <summary>
/// Route definitions for Audit.API service.
/// Internal paths: /audit/*
/// </summary>
public static class AuditRoutes
{
    public const string ServiceName = "audit";

    // Internal paths (used by service)
    public const string Logs = "/audit/logs";
    public const string EntityHistory = "/audit/logs/entity/{entityType}/{entityId}";
    public const string LoginSessions = "/audit/login-sessions";
    public const string LoginSessionById = "/audit/login-sessions/{id:guid}";
    public const string LoginSessionLogout = "/audit/login-sessions/{id:guid}/logout";
    public const string UserSessions = "/audit/login-sessions/user/{userId:guid}";
    public const string ApplicationLogs = "/audit/application-logs";
    public const string ApplicationLogById = "/audit/application-logs/{id:guid}";
    public const string ApplicationLogStats = "/audit/application-logs/stats";

    // Gateway paths (external API)
    public static class Gateway
    {
        public const string Prefix = "/api/audit";
    }
}
