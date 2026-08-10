using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;

namespace IdentityService.Features.Vehicles.GetVehicles;

public class GetVehiclesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/vehicles", ([AsParameters] PaginationRequest request) =>
                ApiResponse.Paginated<object>([], totalCount: 0, request).ToHttpResult())
            .WithTags("Vehicles")
            .WithName("GetVehicles")
            .AllowAnonymous();
    }
}
