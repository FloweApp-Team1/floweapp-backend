using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace IdentityService.Common.Middlewares
{
  
        public sealed class AuditingAuthorizationMiddlewareResultHandler(
         ILogger<AuditingAuthorizationMiddlewareResultHandler> logger)
       : IAuthorizationMiddlewareResultHandler
        {
            private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

            public async Task HandleAsync(
                RequestDelegate next,
                HttpContext context,
                AuthorizationPolicy policy,
                PolicyAuthorizationResult authorizeResult)
            {
                if (!authorizeResult.Succeeded)
                {
                    logger.LogWarning(
                        "Forbidden access attempt. Path: {Path}, IP: {Ip}, UserAgent: {UserAgent}, User: {User}",
                        context.Request.Path,
                        context.Connection.RemoteIpAddress,
                        context.Request.Headers.UserAgent.ToString(),
                        context.User.Identity?.Name  ?? "anonymous");
                }

                await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            }
        }
}

