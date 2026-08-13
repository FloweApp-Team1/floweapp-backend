using Shared.Contracts;
using Shared.Security;
using Shared.Requests;
using Shared.Responses;
using IdentityService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Admin.DriverApplications.ListDriverApplications;

public class ListDriverApplicationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/drivers/applications", async ([AsParameters] PaginationRequest request,
            [FromQuery] DeliveryStatusEnum deliveryStatus
            , CancellationToken cancelationToken,
            [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new ListDriverApplicationQuery(request, deliveryStatus), cancelationToken);

            if (result.IsSuccess)
            {
                return Results.Ok(ApiResponse.Paginated(result.Value.Items, result.Value.TotalCount, request));
            }
            return Results.NotFound(ApiResponse.Fail(result.Error.Message));
        })

            .WithTags("Admin")
            .WithName("ListDriverApplications")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
