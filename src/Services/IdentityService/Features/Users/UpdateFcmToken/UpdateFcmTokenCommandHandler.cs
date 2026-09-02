using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Users.UpdateFcmToken;

public class UpdateFcmTokenCommandHandler(
    ICurrentUserService _currentUser,
    IGenericRepository<UserDeviceToken> _repository,
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

        var deviceToken = await _repository.Query()
            .FirstOrDefaultAsync(x => x.UserId == userId.Value && x.DeviceId == request.DeviceId, cancellationToken);

        if (deviceToken is null)
        {
            deviceToken = new UserDeviceToken
            {
                UserId = userId.Value,
                DeviceId = request.DeviceId,
                FcmToken = request.FcmToken,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(deviceToken, cancellationToken);
        }
        else
        {
            deviceToken.FcmToken = request.FcmToken;
            deviceToken.UpdatedAt = DateTime.UtcNow;
            _repository.Update(deviceToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateFcmTokenResponse>
            .Success(new UpdateFcmTokenResponse());
    }
}
