using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileCommandHandler(
    ICurrentUserService _currentUser,
    IGenericRepository<User> _repository,
    IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateProfileCommand, Result<UpdateProfileResponse>>
{
    public async Task<Result<UpdateProfileResponse>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<UpdateProfileResponse>
                .Failure("Unauthorized");
        }


        var user = await _repository.GetByIdAsync(
            userId.Value,
            cancellationToken);


        if (user is null)
        {
            return Result<UpdateProfileResponse>
                .Failure("User Not Found");
        }


        // Check email uniqueness if email changed
        if (!string.Equals(
                user.Email,
                request.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = await _repository.FirstOrDefaultAsync(
                x => x.Email == request.Email && x.Id != user.Id,
                cancellationToken);

            if (emailExists is not null)
            {
                return Result<UpdateProfileResponse>
                    .Failure("Email already exists");
            }

            user.Email = request.Email;

            // Optional depending on business rule
            user.IsEmailConfirmed = false;
        }



        if (user.PhoneNumber != request.PhoneNumber)
        {
            var phoneExists = await _repository.FirstOrDefaultAsync(
                x => x.PhoneNumber == request.PhoneNumber && x.Id != user.Id,
                cancellationToken);

            if (phoneExists is not null)
            {
                return Result<UpdateProfileResponse>
                    .Failure("Phone number already exists");
            }

            user.PhoneNumber = request.PhoneNumber;
        }


        // Update allowed profile fields
        user.FullName = request.FullName;
        user.Gender = request.Gender;


        if (request.ProfilePictureUrl is not null)
        {
            user.ImageUrl = request.ProfilePictureUrl;
        }


        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return Result<UpdateProfileResponse>
            .Success(new UpdateProfileResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Gender,
            user.ImageUrl
        ));
    }
}