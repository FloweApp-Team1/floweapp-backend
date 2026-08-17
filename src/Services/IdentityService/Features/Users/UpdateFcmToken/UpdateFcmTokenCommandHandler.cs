using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Users.UpdateFcmToken;

public class UpdateFcmTokenCommandHandler(
    ICurrentUserService _currentUser,
    IGenericRepository<User> _repository,
    IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateFcmTokenCommand, Result<UpdateFcmTokenResponse>>
{
    public async Task<Result<UpdateFcmTokenResponse>> Handle(
        UpdateFcmTokenCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<UpdateFcmTokenResponse>
                .Failure(Error.New("Users.Unauthorized", "Unauthorized"));
        }

        var user = await _repository.GetByIdAsync(
            userId.Value,
            cancellationToken);

        if (user is null)
        {
            return Result<UpdateFcmTokenResponse>
                .Failure(Error.New("Users.NotFound", "User not found"));
        }

        user.FcmToken = request.FcmToken;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateFcmTokenResponse>
            .Success(new UpdateFcmTokenResponse());
    }
}
