using IdentityService.Domain.Entities;
using IdentityService.Features.Admin.CreateVehicles.Dtos;
using MediatR;
using Org.BouncyCastle.Crypto.Prng;
using Shared.Interfaces;
using Shared.Results;

namespace IdentityService.Features.Admin.CreateVehicles
{
    public class CreateVehiclesCommandHandler : IRequestHandler<CreateVehiclesCommand, Result<CreateVehicleResponseDto>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;

        public CreateVehiclesCommandHandler(IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
        {
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
        }
        public async Task<Result<CreateVehicleResponseDto>> Handle(CreateVehiclesCommand request, CancellationToken cancellationToken)
        {
            var _reposoitry=unitOfWork.Repository<VehicleType>();
            var adminId = currentUserService.UserId;
            if(adminId == Guid.Empty)
            {
                return Result<CreateVehicleResponseDto>.Failure("UnAuthorized Access");

            }

            var IsNameExist= await _reposoitry.ExistsAsync(e=>e.Name.Equals(request.Name)&&e.IsDeleted==false);
            if (IsNameExist)
            {
                return Result<CreateVehicleResponseDto>.Failure("The Name Of The Vehicle Type is Exist");
            }
            var NewVehicleType = new VehicleType
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy=adminId
                
            };

            await _reposoitry.AddAsync(NewVehicleType);
            await unitOfWork.SaveChangesAsync();


            var response = new CreateVehicleResponseDto
            {
                Id = NewVehicleType.Id,
                Name = NewVehicleType.Name,
                CreatedAt=NewVehicleType.CreatedAt,
                CreatedBy=adminId
            };

            return Result<CreateVehicleResponseDto>.Success(response);

        }
    }
}
