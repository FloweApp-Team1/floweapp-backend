using IdentityService.Common.Contracts;
using IdentityService.Common.Requests;
using IdentityService.Common.Responses;

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
