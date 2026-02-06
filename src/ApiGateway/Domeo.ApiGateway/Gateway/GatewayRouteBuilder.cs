using Auth.Contracts.Routes;
using Audit.Contracts.Routes;
using Clients.Contracts.Routes;
using Materials.Contracts.Routes;
using Modules.Contracts.Routes;
using Projects.Contracts.Routes;
using Yarp.ReverseProxy.Configuration;

namespace Domeo.ApiGateway.Gateway;

/// <summary>
/// Builds YARP RouteConfig[] from centralized route definitions.
/// </summary>
public static class GatewayRouteBuilder
{
    public static RouteConfig[] BuildRoutes()
    {
        var routes = new List<RouteConfig>();

        // Auth routes (no authorization required for login)
        routes.Add(CreateRoute(
            id: "auth",
            gatewayPath: AuthRoutes.Gateway.Prefix,
            serviceName: AuthRoutes.ServiceName,
            removePrefix: "/api",
            requireAuth: false));

        // Clients routes
        routes.Add(CreateRoute(
            id: "clients",
            gatewayPath: ClientsRoutes.Gateway.Prefix,
            serviceName: ClientsRoutes.ServiceName,
            removePrefix: "/api"));

        // Audit routes
        routes.Add(CreateRoute(
            id: "audit",
            gatewayPath: AuditRoutes.Gateway.Prefix,
            serviceName: AuditRoutes.ServiceName,
            removePrefix: "/api"));

        // Projects routes
        routes.Add(CreateRoute(
            id: "projects",
            gatewayPath: ProjectsRoutes.Gateway.ProjectsPrefix,
            serviceName: ProjectsRoutes.ServiceName,
            removePrefix: "/api"));

        routes.Add(CreateRoute(
            id: "rooms",
            gatewayPath: ProjectsRoutes.Gateway.RoomsPrefix,
            serviceName: ProjectsRoutes.ServiceName,
            removePrefix: "/api"));

        routes.Add(CreateRoute(
            id: "edges",
            gatewayPath: ProjectsRoutes.Gateway.EdgesPrefix,
            serviceName: ProjectsRoutes.ServiceName,
            removePrefix: "/api"));

        routes.Add(CreateRoute(
            id: "cabinets",
            gatewayPath: ProjectsRoutes.Gateway.CabinetsPrefix,
            serviceName: ProjectsRoutes.ServiceName,
            removePrefix: "/api"));

        routes.Add(CreateRoute(
            id: "cabinet-parts",
            gatewayPath: ProjectsRoutes.Gateway.CabinetPartsPrefix,
            serviceName: ProjectsRoutes.ServiceName,
            removePrefix: "/api"));

        // Modules routes
        // Main prefix: /api/modules/* -> /* (removes /api/modules)
        routes.Add(CreateRoute(
            id: "modules",
            gatewayPath: ModulesRoutes.Gateway.Prefix,
            serviceName: ModulesRoutes.ServiceName,
            removePrefix: "/api/modules"));

        // Alternative route: /api/assemblies/* -> /assemblies/*
        routes.Add(CreateRouteWithPathRewrite(
            id: "assemblies",
            gatewayPath: ModulesRoutes.Gateway.Assemblies,
            serviceName: ModulesRoutes.ServiceName,
            targetPath: "/assemblies"));

        // Alternative route: /api/module-categories/* -> /categories/*
        routes.Add(CreateRouteWithPathRewrite(
            id: "module-categories",
            gatewayPath: ModulesRoutes.Gateway.ModuleCategories,
            serviceName: ModulesRoutes.ServiceName,
            targetPath: "/categories"));

        // Materials routes
        // Main prefix: /api/materials/* -> /* (removes /api/materials)
        routes.Add(CreateRoute(
            id: "materials",
            gatewayPath: MaterialsRoutes.Gateway.Prefix,
            serviceName: MaterialsRoutes.ServiceName,
            removePrefix: "/api/materials"));

        // Alternative route: /api/suppliers/* -> /suppliers/*
        routes.Add(CreateRouteWithPathRewrite(
            id: "suppliers",
            gatewayPath: MaterialsRoutes.Gateway.Suppliers,
            serviceName: MaterialsRoutes.ServiceName,
            targetPath: "/suppliers"));

        // Alternative route: /api/material-categories/* -> /categories/*
        routes.Add(CreateRouteWithPathRewrite(
            id: "material-categories",
            gatewayPath: MaterialsRoutes.Gateway.MaterialCategories,
            serviceName: MaterialsRoutes.ServiceName,
            targetPath: "/categories"));

        return routes.ToArray();
    }

    private static RouteConfig CreateRoute(
        string id,
        string gatewayPath,
        string serviceName,
        string removePrefix,
        bool requireAuth = true)
    {
        return new RouteConfig
        {
            RouteId = $"{id}-route",
            ClusterId = $"{serviceName}-cluster",
            AuthorizationPolicy = requireAuth ? "default" : null,
            Match = new RouteMatch
            {
                Path = $"{gatewayPath}/{{**catch-all}}"
            },
            Transforms = new List<IReadOnlyDictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    ["PathRemovePrefix"] = removePrefix
                }
            }
        };
    }

    private static RouteConfig CreateRouteWithPathRewrite(
        string id,
        string gatewayPath,
        string serviceName,
        string targetPath,
        bool requireAuth = true)
    {
        return new RouteConfig
        {
            RouteId = $"{id}-route",
            ClusterId = $"{serviceName}-cluster",
            AuthorizationPolicy = requireAuth ? "default" : null,
            Match = new RouteMatch
            {
                Path = $"{gatewayPath}/{{**catch-all}}"
            },
            Transforms = new List<IReadOnlyDictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    ["PathPattern"] = $"{targetPath}/{{**catch-all}}"
                }
            }
        };
    }
}
