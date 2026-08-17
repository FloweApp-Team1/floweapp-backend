using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.GetStore;

public class GetStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/stores/{storeId:guid}", (Guid storeId) =>
                ApiResponse.Success(new { }, "Store retrieved").ToHttpResult())
            .WithTags("Admin - Stores")
            .WithName("GetStoreDetails")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
