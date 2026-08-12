using Shared.Domain;

namespace IdentityService.Domain.Entities
{
    public class VehicleType:BaseEntity
    {
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        public ICollection<VehicleInfo> Vehicles { get; set; } = new List<VehicleInfo>();

        public ICollection<DriverApplication> DriverApplications { get; set; }= new List<DriverApplication>();

    }
}
