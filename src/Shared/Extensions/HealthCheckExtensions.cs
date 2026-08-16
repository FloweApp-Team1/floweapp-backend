using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Responses;

namespace Shared.Extensions
{
    public static class HealthCheckExtensions
    {
        // One probe shape for the whole system: every container exposes an anonymous
        // GET /health returning the standard ApiResponse envelope, so the docker-compose
        // healthcheck, the gateway and a human curl all read the same thing.
        public static IEndpointConventionBuilder MapSharedHealthChecks(
            this IEndpointRouteBuilder endpoints, string pattern = "/health") =>
            endpoints
                .MapHealthChecks(pattern, new HealthCheckOptions { ResponseWriter = WriteResponse })
                // The services set a RequireAuthenticatedUser fallback policy - without this
                // the probe would answer 401 and every container would look unhealthy forever.
                .AllowAnonymous();

        private static Task WriteResponse(HttpContext context, HealthReport report)
        {
            // The middleware has already chosen 200 / 503 from report.Status; the envelope
            // just mirrors it so the body never disagrees with the status code.
            var body = new ApiResponse<object>
            {
                Status = report.Status == HealthStatus.Healthy,
                Code = context.Response.StatusCode,
                Message = report.Status.ToString(),
                Data = new
                {
                    DurationMs = (int)report.TotalDuration.TotalMilliseconds,
                    Checks = report.Entries.ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value.Status.ToString())
                }
            };

            return context.Response.WriteAsJsonAsync(body);
        }
    }
}
