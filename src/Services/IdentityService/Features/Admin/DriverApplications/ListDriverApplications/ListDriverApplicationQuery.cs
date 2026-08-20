using Shared.Models;
using Shared.Results;
using Shared.Requests;
using Shared.Responses;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Features.Admin.DriverApplications.ListDriverApplications.Dtos;
using MediatR;

namespace IdentityService.Features.Admin.DriverApplications.ListDriverApplications
{
    public record ListDriverApplicationQuery(PaginationRequest Pagination, DeliveryStatusEnum? DeliveryStatus = null) 
        : IRequest<Result<PagedResult<DriverApplicationResponseDto>>>;


    
}
