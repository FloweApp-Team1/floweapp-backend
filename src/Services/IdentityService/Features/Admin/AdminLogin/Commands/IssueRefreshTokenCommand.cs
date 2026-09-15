using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using Shared.Results;
using Shared.Settings;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using IdentityService.Features.Auth.Login.Commands;

namespace IdentityService.Features.Admin.AdminLogin.Commands
{
    public sealed record IssueRefreshTokenCommand(Guid UserId) : IRequest<Result<IssuedRefreshToken>>;

    public sealed class IssueRefreshTokenHandler(
      IJwtService jwtService,
      IUnitOfWork unitOfWork,
      IOptions<JwtSettings> jwtOptions)
      : IRequestHandler<IssueRefreshTokenCommand, Result<IssuedRefreshToken>>
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        public async Task<Result<IssuedRefreshToken>> Handle(IssueRefreshTokenCommand request, CancellationToken ct)
        {
            var rawToken = jwtService.GenerateRefreshTokenValue();

            var sessionId = Guid.NewGuid();
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = jwtService.HashRefreshTokenValue(rawToken),
                FamilyId = sessionId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                UserId = request.UserId
            };

            await unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, ct);
            await unitOfWork.SaveChangesAsync(ct);

            // The caller gets the raw value; it is never recoverable from the database.
            return Result.Success(new IssuedRefreshToken(rawToken, sessionId));
        }
    }
}
