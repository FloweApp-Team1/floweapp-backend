using IdentityService.Domain.Enums;

namespace IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication.Dtos_VM
{
    public class ApproveDriverApplicationVM
    {

        public string ApplicationId { get; set; } = null!;

        public string DeliveryId { get; set; } = null!;

        public string Status { get; set; } = null!;


        public string ReviewedBy { get; set; } = null!;

        public DateTime ReviewedAt { get; set; }

    }
    

}
