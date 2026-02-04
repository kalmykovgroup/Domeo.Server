namespace Domeo.Shared.Routes.Routers;

/// <summary>
/// Route definitions for Auth.API service.
/// Internal paths: /auth/*
/// </summary>
public static class AuthRoutes
{
    public const string ServiceName = "auth";

    // Internal paths (used by service)
    public const string Base = "/auth";
    public const string Login = "/auth/login";
    public const string Callback = "/auth/callback";
    public const string Refresh = "/auth/refresh";
    public const string Logout = "/auth/logout";
    public const string Me = "/auth/me";
    public const string Token = "/auth/token";

    // Gateway paths (external API)
    public static class Gateway
    {
        public const string Prefix = "/api/auth";
    }
}
