using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Auth.Login.Commands
{
    public sealed record UpdateFcmTokenCommand(Guid UserId, string DeviceId, string FcmToken) : IRequest<Result>;

    public sealed class UpdateFcmTokenHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateFcmTokenCommand, Result>
    {
        public async Task<Result> Handle(UpdateFcmTokenCommand request, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(request.FcmToken) || string.IsNullOrEmpty(request.DeviceId))
                return Result.Success();

            var repository = unitOfWork.Repository<UserDeviceToken>();
            var token = await repository.Query()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.DeviceId == request.DeviceId, ct);

            if (token == null)
            {
                token = new UserDeviceToken
                {
                    UserId = request.UserId,
                    DeviceId = request.DeviceId,
                    FcmToken = request.FcmToken,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await repository.AddAsync(token, ct);
            }
            else
            {
                token.FcmToken = request.FcmToken;
                token.UpdatedAt = DateTime.UtcNow;
                repository.Update(token);
            }

            return Result.Success();
        }
    }
}
