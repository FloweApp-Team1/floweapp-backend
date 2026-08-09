using IdentityService.Common.Interfaces;
using IdentityService.Common.Results;
using IdentityService.Common.Settings;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace IdentityService.Features.Admin.AdminLogin.Commands
{
    public sealed record IssueRefreshTokenCommand(Guid UserId) : IRequest<Result<string>>;

    public sealed class IssueRefreshTokenHandler(
      IJwtService jwtService,
      IUnitOfWork unitOfWork,
      IOptions<JwtSettings> jwtOptions)
      : IRequestHandler<IssueRefreshTokenCommand, Result<string>>
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        public async Task<Result<string>> Handle(IssueRefreshTokenCommand request, CancellationToken ct)
        {
            var rawToken = jwtService.GenerateRefreshTokenValue();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = jwtService.HashRefreshTokenValue(rawToken),
                FamilyId = Guid.NewGuid(), // new login session starts a fresh token family
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                UserId = request.UserId
            };

            await unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, ct);
            await unitOfWork.SaveChangesAsync(ct);

            // The caller gets the raw value; it is never recoverable from the database.
            return Result.Success(rawToken);
        }
    }
}
