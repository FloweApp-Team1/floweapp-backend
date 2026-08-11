using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using Shared.Results;
using Shared.Settings;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace IdentityService.Features.Auth.Login.Commands
{
    public sealed record IssueUserRefreshTokenCommand(Guid UserId, string IpAddress) : IRequest<Result<string>>;

    public sealed class IssueUserRefreshTokenHandler(
      IJwtService jwtService,
      IUnitOfWork unitOfWork,
      IOptions<JwtSettings> jwtOptions)
      : IRequestHandler<IssueUserRefreshTokenCommand, Result<string>>
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        public async Task<Result<string>> Handle(IssueUserRefreshTokenCommand request, CancellationToken ct)
        {
            var rawToken = jwtService.GenerateRefreshTokenValue();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = jwtService.HashRefreshTokenValue(rawToken),
                FamilyId = Guid.NewGuid(), // Treat as new session
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                IpAddress = request.IpAddress,
                UserId = request.UserId
            };

            await unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success(rawToken);
        }
    }
}
