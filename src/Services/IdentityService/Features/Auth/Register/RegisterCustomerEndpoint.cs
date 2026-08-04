using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Auth.Register;

public class RegisterCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", () =>
                ApiResponse.Success(new { }, "Account created", StatusCodes.Status201Created).ToHttpResult())
            .WithTags("Auth")
            .WithName("RegisterCustomer")
            .AllowAnonymous();
    }
}
