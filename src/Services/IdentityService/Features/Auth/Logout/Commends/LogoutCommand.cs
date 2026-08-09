using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using MediatR;
using RefresheshTokenEntity = IdentityService.Domain.Entities.RefreshToken;

namespace IdentityService.Features.Auth.Logout.Commends
{
    public record LogoutCommand(string RefreshTokenValue) : IRequest<Result>;

    public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public LogoutHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var tokenRepo = _unitOfWork.Repository<RefresheshTokenEntity>();
            var hashedToken = _jwtService.HashRefreshTokenValue(request.RefreshTokenValue);
            var token = await tokenRepo.FirstOrDefaultAsync(t => t.Token == hashedToken, cancellationToken);

            
            if (token is null || token.IsRevoked)
                return Result.Success();

            token.RevokedAt = DateTime.UtcNow;
            tokenRepo.Update(token);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
