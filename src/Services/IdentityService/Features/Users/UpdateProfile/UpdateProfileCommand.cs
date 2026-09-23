using Shared.Results;
using IdentityService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Features.Users.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string PhoneNumber,
    GenderEnum Gender,
    IFormFile? ProfilePicture
) : IRequest<Result<UpdateProfileResponse>>;