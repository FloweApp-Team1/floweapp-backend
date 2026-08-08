using IdentityService.Common.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Auth.Register;

public class RegisterCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterCustomerCommand command , [FromServices] ISender sender) =>
        {
            var response = await sender.Send(command);
            return response.ToHttpResult();
        })
            .WithTags("Auth")
            .WithName("RegisterCustomer")
            .AllowAnonymous();
    }
}
