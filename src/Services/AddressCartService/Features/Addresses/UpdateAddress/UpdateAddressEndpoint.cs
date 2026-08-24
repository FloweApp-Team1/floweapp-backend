using MediatR;
using Shared.Contracts;
using Shared.Responses;
using Shared.Security;
using Shared.Extensions;
namespace AddressCartService.Features.Addresses.UpdateAddress;

public class UpdateAddressEndpoint : IEndpoint
{
  
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/me/addresses/{id:guid}", async (
                Guid id,
                UpdateAddressRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var command = new UpdateAddressCommand(
                id,
                request.RecipientName,
                request.RecipientPhone,
                request.AddressLine,
                request.GovernorateId,
                request.CityId,
                request.Area,
                request.Label,
                request.Lat,
                request.Lng);

            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Address updated successfully.");
        })
            .RequireAuthorization(AppPolicies.CustomerOnly)
            .WithName("UpdateAddress")
            .WithTags("Addresses")
            .WithSummary("Updates an existing address's details and re-resolves the serving store if the location changed.");
    }
}


public sealed record UpdateAddressRequest(
    string RecipientName,
    string RecipientPhone,
    string AddressLine,
    int GovernorateId,
    int CityId,
    string Area,
    string? Label,
    double? Lat,
    double? Lng);
 
