using ApiGateway.Configuration;
using Shared.Extensions;
using Shared.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Every runtime value lives in the Gateway__* environment variables; a missing one
// throws here instead of the gateway starting with a wrong destination.
var gateway = GatewaySettings.Bind(builder.Configuration);

// 1. JWT authentication - the same signing key/issuer/audience the services validate with.
builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// 2. Rate limiting, partitioned per caller so one noisy client cannot exhaust the window
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(ProxyConfiguration.AuthPolicy,
        context => PerClientFixedWindow(context, gateway.RateLimiting.Auth));

    options.AddPolicy(ProxyConfiguration.DefaultPolicy,
        context => PerClientFixedWindow(context, gateway.RateLimiting.Default));

    // 429 is returned in the same ApiResponse envelope as every other failure.
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse.Fail("Too many requests. Please try again later.",
                StatusCodes.Status429TooManyRequests), ct);
    };
});

// 3. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("GlobalCorsPolicy", policy =>
    {
        if (gateway.AllowAnyOrigin)
            policy.AllowAnyOrigin();
        else
            policy.WithOrigins(gateway.Origins).AllowCredentials();

        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

// 4. Reverse proxy. YARP appends X-Forwarded-* on the way out, which is what lets the
builder.Services.AddReverseProxy()
    .LoadFromMemory(ProxyConfiguration.BuildRoutes(), ProxyConfiguration.BuildClusters(gateway));

// 5. The gateway's own probe. It reports the gateway only and never fans out to the
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("GlobalCorsPolicy");

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapReverseProxy();

app.MapSharedHealthChecks();

app.Run();

// Authenticated callers get their own window per user id, so several people behind one
// office NAT no longer share a budget; anonymous traffic still falls back to the IP.
static RateLimitPartition<string> PerClientFixedWindow(HttpContext context, RateLimitWindow window) =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: UserId(context) is { } userId
            ? $"user:{userId}"
            : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = window.PermitLimit,
            Window = TimeSpan.FromSeconds(window.WindowSeconds),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });

// Same claim pair CurrentUserService reads in the services: JwtBearer maps "sub" onto
// NameIdentifier by default, so both spellings have to be tried.
static string? UserId(HttpContext context) =>
    context.User.Identity?.IsAuthenticated == true
        ? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
          ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        : null;
