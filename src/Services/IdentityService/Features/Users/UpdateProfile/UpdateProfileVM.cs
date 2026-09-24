using IdentityService.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileVM
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public GenderEnum Gender { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}
