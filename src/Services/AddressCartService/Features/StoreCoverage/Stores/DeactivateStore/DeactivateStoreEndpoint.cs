using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.DeactivateStore;

public class DeactivateStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/stores/{storeId:guid}", (Guid storeId) =>
                ApiResponse.Success(new { }, "Store deactivated").ToHttpResult())
            .WithTags("Admin - Stores")
            .WithName("DeactivateStore")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
