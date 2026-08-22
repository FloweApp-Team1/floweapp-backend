using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Addresses.CreateAddress;

public class CreateAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/me/addresses", async (
                [FromBody] CreateAddressCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                return result.ToMinimalApiResult("Address created");
            })
            .WithTags("Addresses")
            .WithName("CreateAddress")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
