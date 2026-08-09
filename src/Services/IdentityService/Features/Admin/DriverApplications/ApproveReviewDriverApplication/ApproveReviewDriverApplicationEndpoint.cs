using IdentityService.Common.Contracts;
using IdentityService.Common.Models;
using IdentityService.Common.Responses;
using IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication;
using IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication.Dtos_VM;
using IdentityService.Features.Admin.DriverApplications.RejectReviewDriverApplication.Dtos_VM;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication;

public class ApproveReviewDriverApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/drivers/applications/approve", async (Guid ApplicationId, CancellationToken cancelationToken,
            [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new ApproveDriverApplicationCommand(ApplicationId), cancelationToken);
            if (result.IsSuccess)
            {
                var ApproveVM = new ApproveDriverApplicationVM
                {
                    ApplicationId = result.Value.ApplicationId.ToString(),
                    DeliveryId = result.Value.DeliveryId.ToString(),
                    ReviewedAt = result.Value.ReviewedAt,
                    ReviewedBy = result.Value.ReviewedBy,
                    Status = result.Value.Status.ToString()
                };
                return Results.Created($"/admin/drivers/applications/{ApplicationId.ToString()}", ApiResponse<ApproveDriverApplicationVM>.Success(ApproveVM));
            }
            return Results.BadRequest(ApiResponse<ApproveDriverApplicationVM>.Fail(result.Error));
        })
            .WithTags("Admin")
            .WithName("ApproveReviewDriverApplication");
            //.RequireAuthorization("AdminOnly");
    }
}
