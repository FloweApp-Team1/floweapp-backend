using IdentityService.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities
{
    public class User:BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateOnly? BirthDate { get; set; } 
        public string? ImageUrl { get; set; } 
        public bool IsEmailConfirmed { get; set; }=false;
        public GenderEnum Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string FcmToken { get; set; } = null!;
        public NotifcationStatusEnum NotifcationStatus { get; set; }

        public ICollection<UserRole>? UserRoles { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }



    }
}
