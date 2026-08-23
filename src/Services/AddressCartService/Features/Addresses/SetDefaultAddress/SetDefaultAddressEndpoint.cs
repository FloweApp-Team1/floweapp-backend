using MediatR;
using Shared.Contracts;
using Shared.Extensions;
namespace AddressCartService.Features.Addresses.SetDefaultAddress;

public sealed class SetDefaultAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/users/me/addresses/{id:guid}/default", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new SetDefaultAddressCommand(id), cancellationToken);
            return result.ToMinimalApiResult("Default address updated successfully.");
        })
            .RequireAuthorization()
            .WithName("SetDefaultAddress")
            .WithTags("Addresses")
            .WithSummary("Marks the given address as the current user's default address.");
    }
}
