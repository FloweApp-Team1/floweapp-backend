using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Features.Admin.DriverApplications.GetDriverApplication.Dtos;
using MediatR;

namespace IdentityService.Features.Admin.DriverApplications.GetDriverApplication
{
    public record GetDriverApplicationDetailsQuery(Guid Id) : IRequest<Result<DriverApplicationDetailsDto>>;
}
