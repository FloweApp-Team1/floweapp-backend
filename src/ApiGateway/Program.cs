using Shared.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 1. JWT Authentication
builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

// 2. Rate Limiting
var authLimit = builder.Configuration.GetValue<int>("RateLimiting:AuthLimit", 10);
var authWindow = builder.Configuration.GetValue<int>("RateLimiting:AuthWindowSeconds", 60);

var defaultLimit = builder.Configuration.GetValue<int>("RateLimiting:DefaultLimit", 100);
var defaultWindow = builder.Configuration.GetValue<int>("RateLimiting:DefaultWindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = authLimit;
        opt.Window = TimeSpan.FromSeconds(authWindow);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("DefaultPolicy", opt =>
    {
        opt.PermitLimit = defaultLimit;
        opt.Window = TimeSpan.FromSeconds(defaultWindow);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// 3. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("GlobalCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. YARP Configuration from Env Vars
var identityUrl = builder.Configuration["IdentityServiceUrl"] ?? "http://identityservice:8080";
var catalogUrl = builder.Configuration["CatalogServiceUrl"] ?? "http://catalogservice:8080";

var routes = new[]
{
    new RouteConfig()
    {
        RouteId = "identity-route",
        ClusterId = "identity-cluster",
        Match = new RouteMatch { Path = "/api/identity/{**catch-all}" },
        RateLimiterPolicy = "AuthPolicy",
        Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/api/identity" } } }
    },
    new RouteConfig()
    {
        RouteId = "catalog-route",
        ClusterId = "catalog-cluster",
        Match = new RouteMatch { Path = "/api/catalog/{**catch-all}" },
        RateLimiterPolicy = "DefaultPolicy",
        Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/api/catalog" } } }
    },
    new RouteConfig()
    {
        RouteId = "health-route",
        ClusterId = "health-cluster",
        Match = new RouteMatch { Path = "/health" }
    }
};

var clusters = new[]
{
    new ClusterConfig()
    {
        ClusterId = "identity-cluster",
        LoadBalancingPolicy = "RoundRobin",
        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
        {
            { "identity-dest", new DestinationConfig() { Address = identityUrl } }
        }
    },
    new ClusterConfig()
    {
        ClusterId = "catalog-cluster",
        LoadBalancingPolicy = "RoundRobin",
        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
        {
            { "catalog-dest", new DestinationConfig() { Address = catalogUrl } }
        }
    },
    new ClusterConfig()
    {
        ClusterId = "health-cluster",
        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
        {
            { "health-dest", new DestinationConfig() { Address = identityUrl } } // simple bypass
        }
    }
};

builder.Services.AddReverseProxy()
    .LoadFromMemory(routes, clusters);

var app = builder.Build();

app.UseCors("GlobalCorsPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapGet("/gateway-health", () => Results.Ok("Gateway is healthy"));

app.Run();
