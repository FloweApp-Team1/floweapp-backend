using MediatR;
using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;
using Shared.Security;
using Shared.Extensions;

namespace OrdersService.Features.DriverDelivery.GetAvailableOrders
{
        public sealed class GetAvailableOrdersEndpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet("/drivers/available-orders", async Task<IResult> (
                        [AsParameters] PaginationRequest request,
                        ISender sender,
                        CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetAvailableOrdersQuery(request), cancellationToken);

                    return result.IsSuccess
                        ? ApiResponse.Paginated(
                            result.Value.Orders,
                            result.Value.TotalCount,
                            request,
                            "Available orders retrieved").ToHttpResult()
                        : result.ToMinimalApiResult();
                })
                    .WithTags("Driver Fulfillment")
                    .WithName("GetAvailableOrders")
                    .RequireAuthorization(AppPolicies.DriverApproved)
                    .Produces<ApiResponse<IReadOnlyList<AvailableOrderDto>>>(StatusCodes.Status200OK)
                    .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
                    .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
            }
        }
}
