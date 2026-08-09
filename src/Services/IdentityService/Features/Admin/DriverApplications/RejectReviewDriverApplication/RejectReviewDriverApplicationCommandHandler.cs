using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication
{
    public class RejectReviewDriverApplicationCommandHandler:IRequestHandler<RejectReviewDriverApplicationCommand,Result<RejectReviewDriverApplicationDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHttpContextAccessor httpContext;

        public RejectReviewDriverApplicationCommandHandler(
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContext
            )
        {
            _currentUserService = currentUserService;
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
        }

        public async Task<Result<RejectReviewDriverApplicationDto>> Handle(
            RejectReviewDriverApplicationCommand request,
            CancellationToken cancellationToken)
        {



            var _repository = unitOfWork.Repository<DriverApplication>().Query();
            var rejectReason = request.Reason!.Trim();
            var adminId = _currentUserService.UserId;
            if (adminId is null)
            {
                return Result<RejectReviewDriverApplicationDto>.Failure(
                    "Admin user Unauthorized");
            }
            if (string.IsNullOrWhiteSpace(rejectReason))
            {
                return Result<RejectReviewDriverApplicationDto>.Failure(
                    "Reject reason cannot be empty.");
            }

          
            var reviewedAt=DateTime.UtcNow;

            var affectedRows = await _repository
            .Where(x =>
                x.Id == request.ApplicationId &&
                x.Status == DeliveryStatusEnum.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    DeliveryStatusEnum.Rejected)

                .SetProperty(
                    x => x.RejectReason,
                    rejectReason)

                .SetProperty(
                    x => x.ReviewedBy,
                    adminId)

                .SetProperty(
                    x => x.ReviewedAt,
                    reviewedAt),
                cancellationToken);


            if (affectedRows == 0)
            {
                 var exists = await _repository.AnyAsync(
                x => x.Id == request.ApplicationId,
                cancellationToken);

            if (!exists)
            {
                return Result<RejectReviewDriverApplicationDto>.Failure(
                    "Driver application not found.");
            }

            return Result<RejectReviewDriverApplicationDto>.Failure(
                "Driver application has already been reviewed.");
              
            }

            var response = new RejectReviewDriverApplicationDto
            {
                ApplicationId = request.ApplicationId,
                RejectReason = request.Reason,
                ReviewedAt = reviewedAt,
                ReviewedBy = adminId.ToString(),
                Status = DeliveryStatusEnum.Rejected
            };
         

            return Result<RejectReviewDriverApplicationDto>.Success(response);
        





    }
    }
}
