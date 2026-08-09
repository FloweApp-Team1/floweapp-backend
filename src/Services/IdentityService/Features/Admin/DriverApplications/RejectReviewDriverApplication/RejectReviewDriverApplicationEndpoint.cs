using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using IdentityService.Features.Admin.DriverApplications.RejectReviewDriverApplication.Dtos_VM;
using IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Admin.DriverApplications.RejectReviewDriverApplication;

public class RejectReviewDriverApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/drivers/applications/reject", async (Guid ApplicationId, [FromBody] string Reason, CancellationToken cancelationToken,
            [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new RejectReviewDriverApplicationCommand(ApplicationId, Reason), cancelationToken);
            if (result.IsSuccess)
            {
                var RejectVM = new RejectReviewDriverApplicationVM
                {
                    ApplicationId = result.Value.ApplicationId.ToString(),
                    RejectReason = result.Value.RejectReason,
                    ReviewedAt = result.Value.ReviewedAt,
                    ReviewedBy = result.Value.ReviewedBy,
                    Status = result.Value.Status.ToString()
                };
                return Results.Ok(ApiResponse<RejectReviewDriverApplicationVM>.Success(RejectVM));
            }
            return Results.BadRequest(ApiResponse<RejectReviewDriverApplicationVM>.Fail(result.Error.Message));
        })
            .WithTags("Admin")
            .WithName("RejectReviewDriverApplication");
            //.RequireAuthorization("AdminOnly");
    }
}
