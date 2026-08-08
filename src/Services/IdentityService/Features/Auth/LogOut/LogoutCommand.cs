using IdentityService.Common;
using IdentityService.Common.Results;
using IdentityService.Domain.Intefaces;
using MediatR;

namespace IdentityService.Features.Auth.LogOut
{
   
        
        public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;

     
        public sealed class LogoutHandler(IIdentityUnitOfWork unitOfWork)
            : IRequestHandler<LogoutCommand, Result>
        {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
        {
            var token = await unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, ct);

            if (token is null)
                return Result.Failure(AuthErrors.RefreshTokenNotFound);

            if (token.RevokedAt is not null)
                return Result.Failure(AuthErrors.RefreshTokenAlreadyRevoked);

            if (token.ExpiresAt <= DateTime.UtcNow)
                return Result.Failure(AuthErrors.RefreshTokenExpired);

            token.RevokedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}


