using IdentityService.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities
{
    public class ApplicationUser:IdentityUser<Guid>
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public GenderEnum Gender { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;


    }
}
