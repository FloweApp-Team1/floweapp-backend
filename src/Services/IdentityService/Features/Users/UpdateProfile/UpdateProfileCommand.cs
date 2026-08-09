using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Domain.Enums;
using MediatR;

namespace IdentityService.Features.Users.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    GenderEnum Gender,
    string? ProfilePictureUrl
) : IRequest<Result<UpdateProfileResponse>>;