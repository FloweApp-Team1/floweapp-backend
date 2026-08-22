using AddressCartService.Features.StoreCoverage.Common.Dtos;
using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;
namespace AddressCartService.Features.StoreCoverage.Stores.CreateStore;

public class CreateStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/stores", async (CreateStoreRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CreateStoreCommand(request), cancellationToken);
            return result.ToMinimalApiResult("Store created");
        })
            .WithTags("Admin - Stores")
            .WithName("CreateStore")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
