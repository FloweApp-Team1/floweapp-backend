using IdentityService.Domain.Entities;
using IdentityService.Features.Vehicles.GetVehicles.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Models;
using Shared.Results;

namespace IdentityService.Features.Vehicles.GetVehicles
{
    public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, Result<PagedResult<ListVehiclesDto>>>
    {
        private readonly IUnitOfWork unitOfWork;

        public GetVehiclesQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<Result<PagedResult<ListVehiclesDto>>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
        {
            var _repository = unitOfWork.Repository<VehicleType>().Query();

            var _totalCount= await _repository.CountAsync();
            var VehicleResponse= await _repository.Select(e=>new ListVehiclesDto
            {
                Id = e.Id,
                Name = e.Name,
                CreatedAt=e.CreatedAt,

            }).ToListAsync(cancellationToken);

            var _pagedResult = new PagedResult<ListVehiclesDto>(VehicleResponse, _totalCount);
            return Result<PagedResult<ListVehiclesDto>>.Success(_pagedResult);

        }
    }
}
