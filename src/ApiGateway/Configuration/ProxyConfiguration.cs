using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.Configuration
{
    // YARP routes/clusters are built in code (LoadFromMemory) rather than read from
    // appsettings.json: the path prefixes are part of the gateway's public contract and
    // belong with the code, while the destination addresses are environment-specific and
    // come from GatewaySettings.
    public static class ProxyConfiguration
    {
        public const string AuthPolicy = "AuthPolicy";
        public const string DefaultPolicy = "DefaultPolicy";

        public static RouteConfig[] BuildRoutes() =>
        [
            new RouteConfig
            {
                RouteId = "identity-route",
                ClusterId = "identity-cluster",
                Match = new RouteMatch { Path = "/api/identity/{**catch-all}" },
                RateLimiterPolicy = AuthPolicy,
                Transforms = [new Dictionary<string, string> { ["PathRemovePrefix"] = "/api/identity" }]
            },
            new RouteConfig
            {
                RouteId = "catalog-route",
                ClusterId = "catalog-cluster",
                Match = new RouteMatch { Path = "/api/catalog/{**catch-all}" },
                RateLimiterPolicy = DefaultPolicy,
                Transforms = [new Dictionary<string, string> { ["PathRemovePrefix"] = "/api/catalog" }]
            }
        ];

        public static ClusterConfig[] BuildClusters(GatewaySettings settings) =>
        [
            Cluster("identity-cluster", settings.IdentityServiceUrl, settings),
            Cluster("catalog-cluster", settings.CatalogServiceUrl, settings)
        ];

        private static ClusterConfig Cluster(string clusterId, string address, GatewaySettings settings) => new()
        {
            ClusterId = clusterId,
            LoadBalancingPolicy = "RoundRobin",
            HealthCheck = new HealthCheckConfig
            {
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(settings.HealthCheck.IntervalSeconds),
                    Timeout = TimeSpan.FromSeconds(settings.HealthCheck.TimeoutSeconds),
                    Policy = "ConsecutiveFailures",
                    Path = "/health"
                },
                Passive = new PassiveHealthCheckConfig
                {
                    Enabled = true,
                    Policy = "TransportFailureRate",
                    ReactivationPeriod = TimeSpan.FromSeconds(settings.HealthCheck.IntervalSeconds)
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["ConsecutiveActiveHealthCheckFailuresThreshold"] = "2"
            },
            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
            {
                [$"{clusterId}-destination"] = new DestinationConfig { Address = address }
            }
        };
    }
}
