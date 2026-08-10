using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                .Failure(Error.New("Users.Unauthorized", "Unauthorized"));
        }


        var user = await _repository.GetByIdAsync(
            userId.Value,
            cancellationToken);


        if (user is null)
        {
            return Result<UpdateProfileResponse>
                .Failure(Error.New("Users.NotFound", "User not found"));
        }

        // Update the user's profile with the new information 
        user.UpdateProfile(
          request.FullName,
          request.Email,
          request.PhoneNumber,
          request.Gender,
          request.ProfilePictureUrl);


        try
        {
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("IX_Users_Email") == true || ex.InnerException?.Message.Contains("IX_Users_PhoneNumber") == true)
        {
            return Result<UpdateProfileResponse>
                .Failure(Error.New("Users.Conflict", "Email or Phone number already exists"));
        }

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