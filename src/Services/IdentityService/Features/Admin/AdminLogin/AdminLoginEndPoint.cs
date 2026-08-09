using IdentityService.Common.Contracts;
using IdentityService.Common.Extensions;
using IdentityService.Features.Admin.AdminLogin.Dtos;
using MediatR;

namespace IdentityService.Features.Admin.AdminLogin
{
    
        public sealed class AdminLoginEndPoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost("/auth/admin-login", async (
                    LoginAdminRequestDto request,
                    ISender sender,
                    HttpContext context,
                    CancellationToken ct) =>
                {
                   
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var userAgent = context.Request.Headers.UserAgent.ToString() ?? "unknown";

                   
                    var result = await sender.Send(new AdminLoginCommand(request.Email, request.Password, ipAddress, userAgent), ct);

                    
                    return result.ToMinimalApiResult("Admin logged in successfully");
                })
                .RequireRateLimiting("AdminLoginPerIp")
                .WithName("AdminLogin")
                .WithTags("Admin-Auth")
                .AllowAnonymous();
            }
        }
}
