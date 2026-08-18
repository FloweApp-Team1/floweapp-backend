using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.GetStores;

public class GetStoresEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/stores", () =>
                ApiResponse.Success(new { }, "Stores retrieved").ToHttpResult())
            .WithTags("Admin - Stores")
            .WithName("GetStores")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
