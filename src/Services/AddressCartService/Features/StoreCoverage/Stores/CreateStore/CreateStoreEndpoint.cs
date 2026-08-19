using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.CreateStore;

public class CreateStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/stores", () =>
                ApiResponse.Success(new { }, "Store created").ToHttpResult())
            .WithTags("Admin - Stores")
            .WithName("CreateStore")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
