using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.UpdateStore;

public class UpdateStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/stores/{storeId:guid}", (Guid storeId) =>
                ApiResponse.Success(new { }, "Store updated").ToHttpResult())
            .WithTags("Admin - Stores")
            .WithName("UpdateStore")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
