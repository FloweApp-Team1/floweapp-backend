using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Common.Requests;
using IdentityService.Common.Responses;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Features.Admin.DriverApplications.ListDriverApplications.Dtos;
using MediatR;

namespace IdentityService.Features.Admin.DriverApplications.ListDriverApplications
{
    public record ListDriverApplicationQuery(PaginationRequest  Pagination,DeliveryStatusEnum DeliveryStatus) 
        :IRequest<Result<PagedResult<DriverApplicationResponseDto>>>;


    
}
