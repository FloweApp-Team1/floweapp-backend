using MediatR;
using Shared.Contracts;
using Shared.Responses;
using Shared.Security;
using Shared.Extensions;

namespace AddressCartService.Features.Addresses.GetAddress;

public class GetAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/addresses/{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAddressDetailsQuery(id), cancellationToken);
            return result.ToMinimalApiResult();
        })
            .RequireAuthorization()
            .WithName("GetAddressDetails")
            .WithTags("Addresses")
            .RequireAuthorization(AppPolicies.CustomerOnly)
            .WithSummary("Returns the full details of a specific address owned by the current user.");
    }
    
}
