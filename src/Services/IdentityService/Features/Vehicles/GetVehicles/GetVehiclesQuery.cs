using IdentityService.Features.Admin.DriverApplications.ListDriverApplications.Dtos;
using IdentityService.Features.Vehicles.GetVehicles.Dtos;
using MediatR;
using Shared.Models;
using Shared.Requests;
using Shared.Results;

namespace IdentityService.Features.Vehicles.GetVehicles
{
    public record GetVehiclesQuery(PaginationRequest PaginationRequest) :IRequest<Result<PagedResult<ListVehiclesDto>>>;
    
}
