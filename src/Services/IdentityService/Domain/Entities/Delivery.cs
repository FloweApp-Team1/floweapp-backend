using IdentityService.Domain.Enums;
using System.ComponentModel;

namespace IdentityService.Domain.Entities
{
    public class Delivery:User
    {
        public string NationalIdNumber { get; set; } = null!;

        public string LicenseDocument { get; set; } = null!;
        public DeliveryStatusEnum Status { get; set; }= DeliveryStatusEnum.Pending;

        public VehicleInfo VehicleInfo { get; set; }


    }
}
