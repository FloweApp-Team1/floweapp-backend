using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Auth.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
                [FromBody] LoginRequest request,
                /* [FromHeader(Name = "X-App-Type")] string? appType, */
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                /*
                if (string.IsNullOrWhiteSpace(appType))
                {
                    return ApiResponse.Fail("Missing required header: X-App-Type", StatusCodes.Status400BadRequest).ToHttpResult();
                }
                */

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var command = new LoginCommand(request, /* appType, */ ipAddress);
                var result = await mediator.Send(command, cancellationToken);

                if (!result.IsSuccess)
                {
                    if (result.Error != null && result.Error.StartsWith("RoleAuthorizationFailed"))
                    {
                        return ApiResponse.Fail(result.Error, StatusCodes.Status403Forbidden).ToHttpResult();
                    }
                    
                    return ApiResponse.Fail(result.Error ?? "Authentication failed", StatusCodes.Status401Unauthorized).ToHttpResult();
                }

                return ApiResponse.Success(result.Value, "Logged in").ToHttpResult();
            })
            .WithTags("Auth")
            .WithName("Login")
            .AllowAnonymous();
    }
}
