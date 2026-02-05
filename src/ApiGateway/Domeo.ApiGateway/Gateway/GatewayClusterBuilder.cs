using Auth.Contracts.Routes;
using Audit.Contracts.Routes;
using Clients.Contracts.Routes;
using Materials.Contracts.Routes;
using Modules.Contracts.Routes;
using Projects.Contracts.Routes;
using Yarp.ReverseProxy.Configuration;

namespace Domeo.ApiGateway.Gateway;

/// <summary>
/// Builds YARP ClusterConfig[] from IConfiguration "Services" section.
/// </summary>
public static class GatewayClusterBuilder
{
    public static ClusterConfig[] BuildClusters(IConfiguration configuration)
    {
        var services = configuration.GetSection("Services");

        return
        [
            CreateCluster(AuthRoutes.ServiceName, services["Auth"]),
            CreateCluster(ClientsRoutes.ServiceName, services["Clients"]),
            CreateCluster(AuditRoutes.ServiceName, services["Audit"]),
            CreateCluster(ProjectsRoutes.ServiceName, services["Projects"]),
            CreateCluster(ModulesRoutes.ServiceName, services["Modules"]),
            CreateCluster(MaterialsRoutes.ServiceName, services["Materials"])
        ];
    }

    private static ClusterConfig CreateCluster(string serviceName, string? address)
    {
        if (string.IsNullOrEmpty(address))
            throw new InvalidOperationException($"Services:{serviceName} not configured in appsettings.json");

        return new ClusterConfig
        {
            ClusterId = $"{serviceName}-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                [$"{serviceName}-api"] = new() { Address = address }
            }
        };
    }
}
