using IdentityService.Domain.Enums;

namespace IdentityService.Features.Users.GetProfile;

public sealed record GetProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    GenderEnum Gender,
    string? ProfilePictureUrl
);
