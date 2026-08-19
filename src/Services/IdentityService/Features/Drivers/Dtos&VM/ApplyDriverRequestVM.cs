using IdentityService.Domain.Enums;

namespace IdentityService.Features.Drivers.Dtos_VM
{
    public class ApplyDriverRequestVM
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public GenderEnum Gender { get; set; }

        public string VehiclePlateNumber { get; set; } = default!;
        public Guid VehicleTypeId { get; set; }
        public int VehicleCapacity { get; set; }

        public IFormFile LicenceImage { get; set; } = default!;

        public string Nid { get; set; } = default!;
        public IFormFile NidImage { get; set; } = default!;

        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
        public string FcmToken { get; set; }= default!;


    }
}
