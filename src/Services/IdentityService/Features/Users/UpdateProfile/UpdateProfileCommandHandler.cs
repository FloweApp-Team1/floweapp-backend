using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileCommandHandler(
    ICurrentUserService _currentUser,
    IGenericRepository<User> _repository,
    IUnitOfWork _unitOfWork,
    IImageService _imageService)
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

        // Upload new profile picture if supplied
        string? newProfilePicturePath = null;
        if (request.ProfilePicture is not null)
        {
            newProfilePicturePath = await _imageService.UploadImageAsync(
                request.ProfilePicture,
                "Users",
                userId.Value.ToString(),
                cancellationToken);
        }

        try
        {
            var oldProfilePicturePath = user.ImageUrl;

            // Update user profile. If no new picture was uploaded, keep existing ImageUrl unchanged.
            user.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Gender,
                newProfilePicturePath ?? user.ImageUrl);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Delete the old profile picture only after successful DB persistence
            if (newProfilePicturePath is not null && !string.IsNullOrWhiteSpace(oldProfilePicturePath))
            {
                await _imageService.DeleteImageAsync(oldProfilePicturePath, cancellationToken);
            }

            return Result<UpdateProfileResponse>
                .Success(new UpdateProfileResponse(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.Gender,
                    user.ImageUrl
                ));
        }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("IX_Users_PhoneNumber") == true)
        {
            // Roll back the newly uploaded file if DB save failed
            if (newProfilePicturePath is not null)
            {
                try
                {
                    await _imageService.DeleteImageAsync(newProfilePicturePath, cancellationToken);
                }
                catch
                {
                    // Do not mask the original database exception
                }
            }

            return Result<UpdateProfileResponse>
                .Failure(Error.New("Users.Conflict", "Phone number already exists"));
        }
        catch (Exception)
        {
            // Roll back the newly uploaded file if DB save failed
            if (newProfilePicturePath is not null)
            {
                try
                {
                    await _imageService.DeleteImageAsync(newProfilePicturePath, cancellationToken);
                }
                catch
                {
                    // Do not mask the original database exception
                }
            }

            return Result<UpdateProfileResponse>
                .Failure(Error.New("Users.UpdateFailed", "Failed to update profile. Please try again."));
        }
    }
}