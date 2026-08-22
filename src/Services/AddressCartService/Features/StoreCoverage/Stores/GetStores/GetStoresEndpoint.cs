using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Requests;
using Shared.Responses;
using Shared.Security;
namespace AddressCartService.Features.StoreCoverage.Stores.GetStores;

public class GetStoresEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/stores", async (
                [AsParameters] ListStoresQueryParams queryParams,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetStoresQuery(queryParams.Page, queryParams.Limit), cancellationToken);

            if (result.IsFailure)
                return result.ToMinimalApiResult("Stores retrieved");
            var pagination = new PaginationRequest(queryParams.Page, queryParams.Limit);
            return ApiResponse
                .Paginated(result.Value.Items, result.Value.TotalCount, pagination, "Stores retrieved")
                .ToHttpResult();
        })
            .WithTags("Admin - Stores")
            .WithName("GetStores")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
