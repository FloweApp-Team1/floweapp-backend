using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using Shared.Responses;
using IdentityService.Domain.Entities;
using IdentityService.Features.Admin.DriverApplications.ListDriverApplications.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Admin.DriverApplications.ListDriverApplications
{
    public class ListDriverApplicationQueryHandler : IRequestHandler<ListDriverApplicationQuery, Result<PagedResult<DriverApplicationResponseDto>>>
    {
        private readonly IUnitOfWork unitOfWork;

        public ListDriverApplicationQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<Result<PagedResult<DriverApplicationResponseDto>>> Handle(ListDriverApplicationQuery request, CancellationToken cancellationToken)
        {
            var repository = unitOfWork.Repository<DriverApplication>();

            var query = repository.Query();

            if (request.DeliveryStatus.HasValue)
            {
                query = query.Where(e => e.Status == request.DeliveryStatus.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var driverApplications = await query
                .OrderByDescending(da => da.SubmittedAt)
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(da => new DriverApplicationResponseDto
                {
                    Id = da.Id.ToString(),
                    Name = $"{da.FirstName} {da.LastName}",
                    Email = da.Email,
                    Phone = da.Phone,
                    VehicleType = da.VehicleType.Name,
                    SubmittedAt = da.SubmittedAt,
                    Status = da.Status
                }).ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<DriverApplicationResponseDto>(driverApplications, totalCount);



            return Result<PagedResult<DriverApplicationResponseDto>>.Success(pagedResult);



        }
    }
}
