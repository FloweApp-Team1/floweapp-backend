using IdentityService.Domain.Enums;

namespace IdentityService.Features.Admin.DriverApplications.RejectReviewDriverApplication.Dtos_VM
{
    public class RejectReviewDriverApplicationVM
    {
        public string ApplicationId { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string RejectReason { get; set; }= null!;

        public string ReviewedBy { get; set; } = null!;

        public DateTime ReviewedAt { get; set; }
    }
}
