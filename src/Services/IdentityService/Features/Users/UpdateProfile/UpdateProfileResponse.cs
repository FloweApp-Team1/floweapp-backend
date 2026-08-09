using IdentityService.Domain.Enums;
namespace IdentityService.Features.Users.UpdateProfile;

public sealed record UpdateProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    GenderEnum Gender,
    string? ProfilePictureUrl
);