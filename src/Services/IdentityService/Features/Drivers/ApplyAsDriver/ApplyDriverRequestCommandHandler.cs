using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Features.Drivers.Dtos_VM;
using MediatR;

namespace IdentityService.Features.Drivers.ApplyAsDriver
{
    public class ApplyDriverRequestCommandHandler : IRequestHandler<ApplyDriverRequestCommand, Result<ApplyDriverDto>>
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IUnitOfWork unitOfWork;
        private readonly IImageService imageService;

        public ApplyDriverRequestCommandHandler(IPasswordHasher passwordHasher, IUnitOfWork unitOfWork,IImageService imageService)
        {
            this.passwordHasher = passwordHasher;
            this.unitOfWork = unitOfWork;
            this.imageService = imageService;
        }
        public async Task<Result<ApplyDriverDto>> Handle(ApplyDriverRequestCommand request, CancellationToken cancellationToken)
        {
            var _repository = unitOfWork.Repository<Delivery>();
            var email = request.Email.Trim().ToLower();
            var IsExistingUser = _repository.Exists(u => u.Email == email);
            if (!IsExistingUser)
            {
                return Result<ApplyDriverDto>.Failure("User with this email already exists.");
            }

            var hashedPassword = passwordHasher.Hash(request.Password);
            var LicenseImg =await imageService.UploadImageAsync(request.LicenceImage,"Drivers",$"{request.Name}{request.Nid}",cancellationToken);
            var NIImg =await imageService.UploadImageAsync(request.NidImage,"Drivers",$"{request.Name}{request.Nid}",cancellationToken);
            var delivery = new Delivery
            {
                FullName = request.Name,
                Email = email,
                PhoneNumber = request.Phone,
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                FcmToken = request.FcmToken,
                PasswordHash = hashedPassword,
                NationalIdNumber = request.Nid,
                LicenseDocument = LicenseImg,
                NIImage = NIImg,
                Status = DeliveryStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow,
                IsActive = false,
                VehicleInfo = new VehicleInfo
                {
                    PlateNumber = request.VehiclePlateNumber,
                    Type = request.VehicleType,
                    Capacity = request.VehicleCapacity,
                },

            };

            await _repository.AddAsync(delivery, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

             return Result<ApplyDriverDto>.Success(new ApplyDriverDto
             {
                 Id = delivery.Id.ToString(),
                 Name = delivery.FullName,
                 Email = delivery.Email,
                 Phone = delivery.PhoneNumber,
                 Gender = delivery.Gender.ToString(),
                 Role = "Driver",
                 CreatedAt =DateTime.UtcNow,
                 UpdatedAt = DateTime.UtcNow,
                 NotifcationStatus = delivery.NotifcationStatus.ToString()
                 

             });

        }
    }
    
}
