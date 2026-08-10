using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using IdentityService.Domain.Entities;
using IdentityService.Features.Admin.DriverApplications.GetDriverApplication.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Admin.DriverApplications.GetDriverApplication
{
    public class GetDriverApplicationDetailsQueryHandler : IRequestHandler<GetDriverApplicationDetailsQuery, Result<DriverApplicationDetailsDto>>
    {
        private readonly IUnitOfWork unitOfWork;

        public GetDriverApplicationDetailsQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<Result<DriverApplicationDetailsDto>> Handle(GetDriverApplicationDetailsQuery request, CancellationToken cancellationToken)
        {
            var _repository = unitOfWork.Repository<DriverApplication>();

            var driverApplication = _repository.Query();

            var driverApplicationDetails = await driverApplication
                .Where(da => da.Id == request.Id)
                .Select(da => new DriverApplicationDetailsDto
                {
                    Id = da.Id,
                    Name = da.Name,
                    Email = da.Email,
                    Phone = da.Phone,
                    Gender = da.Gender,
                    VehicleCapacity = da.VehicleCapacity,
                    VehiclePlateNumber = da.VehiclePlateNumber,
                    Nid= da.NationalId,
                    LicenceImageUrl= da.LicenceImageUrl,
                    NidImageUrl= da.NationalIdImageUrl,
                    Status=da.Status,
                    SubmittedAt=da.SubmittedAt,
                    VehicleType=da.VehicleType,
                    
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (driverApplicationDetails == null)
            {
                return Result<DriverApplicationDetailsDto>.Failure("Not Found");
            }

            return Result<DriverApplicationDetailsDto>.Success(driverApplicationDetails);
        }
    }
}
