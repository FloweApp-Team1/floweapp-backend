using IdentityService.Common.Results;
using IdentityService.Common.Security;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Features.Admin.AdminLogin.Commands
{
    public sealed record IssueRefreshTokenCommand(Guid UserId) : IRequest<Result<string>>;
    public sealed class IssueRefreshTokenHandler(
      
      ITokenService tokenService,
      IIdentityUnitOfWork unitOfWork)
      : IRequestHandler<IssueRefreshTokenCommand, Result<string>>
    {
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

        public async Task<Result<string>> Handle(IssueRefreshTokenCommand request, CancellationToken ct)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = tokenService.GenerateRefreshTokenValue(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
                UserId = request.UserId
            };

            await unitOfWork.RefreshTokens.AddAsync(refreshToken, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success(refreshToken.Token);
        }
    }
}
