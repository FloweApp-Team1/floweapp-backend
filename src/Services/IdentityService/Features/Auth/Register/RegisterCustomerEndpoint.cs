using IdentityService.Common.Contracts;

namespace IdentityService.Features.Auth.Register;

public class RegisterCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", () => { })
            .WithTags("Auth")
            .WithName("RegisterCustomer")
            .AllowAnonymous();
    }
}