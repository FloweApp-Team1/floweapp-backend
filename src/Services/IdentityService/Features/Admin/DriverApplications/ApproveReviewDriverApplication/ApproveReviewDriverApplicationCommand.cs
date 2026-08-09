using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication.Dtos_VM;
using MediatR;

namespace IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication
{
    public record ApproveDriverApplicationCommand(
     Guid ApplicationId
 ) : IRequest<Result<ApproveDriverApplicationDto>>;

}
