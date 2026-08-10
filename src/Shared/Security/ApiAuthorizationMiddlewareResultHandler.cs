using Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Shared.Security
{
    // The single IAuthorizationMiddlewareResultHandler: renders 401/403 in the same
    // ApiResponse envelope as every other failure, and audits the denial.
    public sealed class ApiAuthorizationMiddlewareResultHandler(
        ILogger<ApiAuthorizationMiddlewareResultHandler> logger)
        : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _fallback = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Succeeded)
            {
                await next(context);
                return;
            }

            logger.LogWarning(
                "Authorization denied. Path: {Path}, IP: {Ip}, UserAgent: {UserAgent}, User: {User}",
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                context.Request.Headers.UserAgent.ToString(),
                context.User.Identity?.Name ?? "anonymous");

            if (authorizeResult.Challenged) // no token / invalid token
            {
                await Write(context, ApiResponse.Fail(
                    "Authentication required", StatusCodes.Status401Unauthorized));
                return;
            }

            if (authorizeResult.Forbidden) // valid token, insufficient permission
            {
                var reasons = authorizeResult.AuthorizationFailure?.FailureReasons
                    .Select(r => r.Message)
                    .ToList() ?? [];

                var message = reasons.Contains(DriverApprovedHandler.NotApprovedReason)
                    ? "Your driver application has not been approved yet"
                    : "You do not have permission to perform this action";

                await Write(context, ApiResponse.Fail(message, StatusCodes.Status403Forbidden));
                return;
            }

            await _fallback.HandleAsync(next, context, policy, authorizeResult);
        }

        private static Task Write(HttpContext context, ApiResponse<object> response)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.Code;
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
