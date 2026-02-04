namespace Domeo.Shared.Routes.Routers;

/// <summary>
/// Route definitions for Clients.API service.
/// Internal paths: /clients/*
/// </summary>
public static class ClientsRoutes
{
    public const string ServiceName = "clients";

    // Internal paths (used by service)
    public const string Base = "/clients";
    public const string ById = "/clients/{id:guid}";
    public const string Restore = "/clients/{id:guid}/restore";

    // Gateway paths (external API)
    public static class Gateway
    {
        public const string Prefix = "/api/clients";
    }
}
