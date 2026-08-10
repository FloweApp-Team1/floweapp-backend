using Shared.Models;
using Shared.Results;
using IdentityService.Domain.Enums;
using MediatR;

namespace IdentityService.Features.Users.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    GenderEnum Gender,
    string? ProfilePictureUrl
) : IRequest<Result<UpdateProfileResponse>>;