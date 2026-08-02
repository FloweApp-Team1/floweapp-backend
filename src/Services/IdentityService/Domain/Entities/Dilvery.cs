using IdentityService.Domain.Enums;
using System.ComponentModel;

namespace IdentityService.Domain.Entities
{
    public class Dilvery
    {
        public Guid Id { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Guid UserId { get; set; } 
        public string NationalIdNumber { get; set; } = null!;

        public string VehiclePlateNumber { get; set; } = null!;
        public string LicenseDocument { get; set; } = null!;
        public StatusEnum Status { get; set; }= StatusEnum.Pending;

        public VehicleInfo VehicleInfo { get; set; }
        public Guid VehicleId { get; set; } 


    }
}
