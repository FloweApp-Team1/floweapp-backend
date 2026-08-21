using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;
using Shared.Security;
using System.Threading.Tasks;

namespace OrdersService.Features.Orders.GetOrders;

public class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async Task<IResult> ([AsParameters] PaginationRequest request, ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersQuery(request));

            if (result.IsSuccess)
            {
                return ApiResponse.Paginated(
                    result.Value.Orders,
                    result.Value.TotalCount,
                    request,
                    "Orders retrieved"
                ).ToHttpResult();
            }

            return ApiResponse.Fail(result.Error.Message, StatusCodes.Status400BadRequest).ToHttpResult();
        })
        .WithTags("Orders")
        .WithName("GetOrders")
        .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
