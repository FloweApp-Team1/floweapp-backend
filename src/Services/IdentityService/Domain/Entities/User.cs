using IdentityService.Domain.Enums;

using Shared.Domain;
namespace IdentityService.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public GenderEnum Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? FcmToken { get; set; }
        public NotificationStatusEnum NotificationStatus { get; set; } = NotificationStatusEnum.on;

        public ICollection<UserRole>? UserRoles { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }


        public void UpdateProfile(
     string firstName,
     string lastName,
     string phoneNumber,
     GenderEnum gender,
     string? profilePictureUrl)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Gender = gender;
            ImageUrl = profilePictureUrl;
        }
    }
}
