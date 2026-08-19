using IdentityService.Domain.Enums;

namespace IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication.Dtos
{
    public class RejectReviewDriverApplicationDto
    {
        public Guid ApplicationId { get; set; }

        public DeliveryStatusEnum Status { get; set; }

        public string RejectReason { get; set; } = null!;

        public string ReviewedBy { get; set; } = null!;

        public DateTime ReviewedAt { get; set; }
    }
}
