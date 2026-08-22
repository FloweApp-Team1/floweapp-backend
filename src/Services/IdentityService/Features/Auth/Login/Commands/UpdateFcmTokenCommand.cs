using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Auth.Login.Commands
{
    public sealed record UpdateFcmTokenCommand(Guid UserId, string FcmToken) : IRequest<Result>;

    public sealed class UpdateFcmTokenHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateFcmTokenCommand, Result>
    {
        public async Task<Result> Handle(UpdateFcmTokenCommand request, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(request.FcmToken))
                return Result.Success();

            await unitOfWork.Repository<User>().Query()
                .Where(u => u.Id == request.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.FcmToken, request.FcmToken), ct);

            return Result.Success();
        }
    }
}
