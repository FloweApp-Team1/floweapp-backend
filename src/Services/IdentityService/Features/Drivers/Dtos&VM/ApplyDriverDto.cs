using IdentityService.Domain.Enums;

namespace IdentityService.Features.Drivers.Dtos_VM
{
    public class ApplyDriverDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }
        public string Gender { get; set; } = null!;
        public string NotifcationStatus { get; set; } = null!;

    }
}
