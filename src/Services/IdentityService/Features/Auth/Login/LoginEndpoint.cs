using Shared.Contracts;
using Shared.Responses;
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
                HttpContext httpContext,
                ISender sender,
                CancellationToken cancellationToken) =>
            {

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var command = new LoginCommand(request, ipAddress);
                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                {
                    return ApiResponse.Fail(
                        result.Error.Message, StatusCodes.Status401Unauthorized).ToHttpResult();
                }

                return ApiResponse.Success(result.Value, "Logged in").ToHttpResult();
            })
            .WithTags("Auth")
            .WithName("Login")
            .AllowAnonymous();
    }
}
