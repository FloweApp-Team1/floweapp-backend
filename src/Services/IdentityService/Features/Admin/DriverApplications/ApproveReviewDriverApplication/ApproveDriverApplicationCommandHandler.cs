using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication.Dtos_VM;
using IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication.Dtos;
using IdentityService.Infrastructure.Repositories;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Events.DriverApprovedEvents;
using Shared.Interfaces;
using Shared.Results;
using Shared.Security;
using System.Security.Claims;

namespace IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication
{
    public class ApproveDriverApplicationCommandHandler : IRequestHandler< ApproveDriverApplicationCommand, Result<ApproveDriverApplicationDto>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;

        public ApproveDriverApplicationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService )
        {
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
        }   
        public async Task<Result<ApproveDriverApplicationDto>> Handle(ApproveDriverApplicationCommand request, CancellationToken cancellationToken)
        {
            var _repository = unitOfWork.Repository<DriverApplication>();
            var _deliveryRepository = unitOfWork.Repository<Delivery>();
            var _roleRepository = unitOfWork.Repository<Role>();
            var _userRoleRepository = unitOfWork.Repository<UserRole>();
            var query = _repository.Query();

            var adminId = currentUserService.UserId;
            if (adminId is null)
            {
                return Result<ApproveDriverApplicationDto>.Failure(
                    "Admin user Unauthorized");
            }
            var reviewedAt = DateTime.UtcNow;

            var application = await query.FirstOrDefaultAsync(e => e.Id.Equals(request.ApplicationId),cancellationToken);

            if (application is null)
            {
                return Result<ApproveDriverApplicationDto>.Failure(
                    "Driver application not found.");
            }
            if (application.Status != DeliveryStatusEnum.Pending)
            {
                return Result<ApproveDriverApplicationDto>.Failure(
                    $"Application has already been reviewed. " +
                    $"Current status: {application.Status}.");
            }
             await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var delivery = new Delivery
                {
                    FirstName = application.FirstName,
                    LastName = application.LastName,
                    Email = application.Email,
                    Gender = application.Gender,
                    IsActive = true,
                    LicenseDocument = application.LicenceImageUrl,
                    NationalIdNumber = application.NationalId,
                    NIImage=application.NationalIdImageUrl,
                    PasswordHash = application.PasswordHash,
                    PhoneNumber=application.Phone,
                    NotificationStatus=application.NotificationStatus,
                    Status=DeliveryStatusEnum.Approved,
                    VehicleInfo=new VehicleInfo
                    {
                        Capacity=application.VehicleCapacity,
                        PlateNumber=application.VehiclePlateNumber,
                        VehicleTypeId=application.VehicleTypeId
                    },
                    CreatedAt = DateTime.UtcNow

                };


                await _deliveryRepository.AddAsync(delivery, cancellationToken);


                var driverRole = await _roleRepository.FirstOrDefaultAsync(x => x.Name == AppRoles.Driver, cancellationToken);

                if (driverRole is null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<ApproveDriverApplicationDto>.Failure(
                            "Driver role not found.");
                }
                var userRole = new UserRole
                {
                    UserId = delivery.Id,
                    RoleId = driverRole.Id
                };  

                await _userRoleRepository.AddAsync(userRole, cancellationToken);

                var affectedRows = await query.Where(x => x.Id == request.ApplicationId && x.Status == DeliveryStatusEnum.Pending)
                            .ExecuteUpdateAsync(setters => setters
                            .SetProperty(
                                x => x.Status,
                                DeliveryStatusEnum.Approved)

                            .SetProperty(
                                x => x.ReviewedBy,
                                adminId)

                            .SetProperty(
                                x => x.ReviewedAt,
                                reviewedAt),
                            cancellationToken);

                if (affectedRows == 0)
                {
                    var exists = await query.AnyAsync(x => x.Id == request.ApplicationId, cancellationToken);

                    await unitOfWork.RollbackTransactionAsync(cancellationToken);

                    if (!exists)
                    {
                        return Result<ApproveDriverApplicationDto>.Failure("Driver application not found.");
                    }
                    return Result<ApproveDriverApplicationDto>.Failure("Application has already been reviewed.");
                }
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                

                var response = new ApproveDriverApplicationDto
                {
                    ApplicationId = request.ApplicationId,
                    DeliveryId = delivery.Id,
                    ReviewedAt = reviewedAt,
                    ReviewedBy = adminId.ToString(),
                    Status = delivery.Status
                };

                return Result<ApproveDriverApplicationDto>.Success(
                    response);

            }
            catch (Exception ex)
            {

                await unitOfWork.RollbackTransactionAsync(
                   cancellationToken);
                return Result<ApproveDriverApplicationDto>.Failure(
                       $"{ex.Message}\n{ex.InnerException?.ToString()}");

            }

         



        }
    }
}
