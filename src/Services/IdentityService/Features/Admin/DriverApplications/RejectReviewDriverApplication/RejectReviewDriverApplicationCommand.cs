using IdentityService.Common.Models;
using IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication.Dtos;
using MediatR;

namespace IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication
{
    public record RejectReviewDriverApplicationCommand(
     Guid ApplicationId,
     string Reason
 ) : IRequest<Result<RejectReviewDriverApplicationDto>>;
}
