using MediatR;
using Shared.Contracts;
using Shared.Extensions;

namespace AddressCartService.Features.Addresses.DeleteAddress;

public class DeleteAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/me/addresses/{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteAddressCommand(id), cancellationToken);
            return result.ToMinimalApiResult("Address deleted successfully.");
        })
            .RequireAuthorization()
            .WithName("DeleteAddress")
            .WithTags("Addresses")
            .WithSummary("Deletes an address; auto-reassigns the default if the deleted address was the default.");
    }
}
