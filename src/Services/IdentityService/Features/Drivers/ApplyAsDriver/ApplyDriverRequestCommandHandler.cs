using IdentityService.Common.Contracts;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Features.Drivers.Dtos_VM;
using MediatR;

namespace IdentityService.Features.Drivers.ApplyAsDriver
{
    public class ApplyDriverRequestCommandHandler : IRequestHandler<ApplyDriverRequestCommand, Result<ApplyDriverResponseDto>>
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
        public async Task<Result<ApplyDriverResponseDto>> Handle(ApplyDriverRequestCommand request, CancellationToken cancellationToken)
        {
            var _repository = unitOfWork.Repository<DriverApplication>();
            var _userRepository = unitOfWork.Repository<User>();
            var email = request.Email.Trim().ToLower();
            var IsExistingUser = await _userRepository.ExistsAsync(u => u.Email == email);
            if (IsExistingUser)
            {
                return Result<ApplyDriverResponseDto>.Failure("User with this email already exists.");
            }

            var userId=Guid.NewGuid();
            var hashedPassword = passwordHasher.Hash(request.Password);
            var LicenseImg = await imageService.UploadImageAsync(request.LicenceImage, "Drivers",userId.ToString(), cancellationToken);
            var NIImg = await imageService.UploadImageAsync(request.NidImage, "Drivers", userId.ToString(), cancellationToken);
            try
            {
                var deliveryApplication = new DriverApplication
                {
                    Id = userId,
                    Name = request.Name,
                    Email = email,
                    Phone = request.Phone,
                    Gender = request.Gender,
                    PasswordHash = hashedPassword,
                    VehiclePlateNumber = request.VehiclePlateNumber,
                    VehicleType = request.VehicleType,
                    VehicleCapacity = request.VehicleCapacity,
                    LicenceImageUrl = LicenseImg,
                    NationalId = request.Nid,
                    NationalIdImageUrl = NIImg,
                    Status = DeliveryStatusEnum.Pending,
                    SubmittedAt = DateTime.UtcNow
                    
                };

                await _repository.AddAsync(deliveryApplication, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<ApplyDriverResponseDto>.Success(new ApplyDriverResponseDto
                {
                    Id = deliveryApplication.Id.ToString(),
                    Name = deliveryApplication.Name,
                    Email = deliveryApplication.Email,
                    Phone = deliveryApplication.Phone,
                    Gender = deliveryApplication.Gender.ToString(),
                    CreatedAt = deliveryApplication.SubmittedAt,
                    UpdatedAt = DateTime.UtcNow,
                    NotifcationStatus = deliveryApplication.NotificationStatus.ToString()


                });

            }
            catch (Exception ex)
            {
                await imageService.DeleteImageAsync(LicenseImg, cancellationToken);
                await imageService.DeleteImageAsync(NIImg, cancellationToken);

                return Result<ApplyDriverResponseDto>.Failure($"{ex.Message} \n {ex.InnerException?.ToString()}");

            }


        }
    }
    
}
