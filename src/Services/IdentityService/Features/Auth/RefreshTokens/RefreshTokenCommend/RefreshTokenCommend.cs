using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Common.Settings;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RefreshTokenEntity = IdentityService.Domain.Entities.RefreshToken;

namespace IdentityService.Features.Auth.RefreshTokens
{
    public record RefreshTokenCommand(
        string RefreshTokenValue,
        string? IpAddress,
        string? DeviceName) : IRequest<Result<TokenResponse>>;

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

        public RefreshTokenHandler(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var tokenRepo = _unitOfWork.Repository<RefreshTokenEntity>();
            var hashedIncoming = _jwtService.HashRefreshTokenValue(request.RefreshTokenValue);
            var existing = await tokenRepo.FirstOrDefaultAsync(t => t.Token == hashedIncoming, cancellationToken);

            if (existing is null)
                return Result<TokenResponse>.Failure("Invalid refresh token.");

            // Replaying an already-rotated/revoked token is a compromise signal:
            // revoke the whole family so every device has to log in again.
            if (existing.IsRevoked)
            {
                var family = tokenRepo.Query()
                    .Where(t => t.FamilyId == existing.FamilyId && t.RevokedAt == null)
                    .ToList();

                foreach (var t in family)
                {
                    t.RevokedAt = DateTime.UtcNow;
                    tokenRepo.Update(t);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<TokenResponse>.Failure("Session invalid. Please log in again.");
            }

            if (existing.IsExpired)
                return Result<TokenResponse>.Failure("Refresh token expired. Please log in again.");

            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(existing.UserId, cancellationToken);
            if (user is null || user.IsDeleted || !user.IsActive)
                return Result<TokenResponse>.Failure("Invalid refresh token.");

            var newRawRefreshToken = _jwtService.GenerateRefreshTokenValue();
            var newRefreshToken = new RefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = _jwtService.HashRefreshTokenValue(newRawRefreshToken),
                FamilyId = existing.FamilyId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                DeviceName = request.DeviceName ?? existing.DeviceName,
                IpAddress = request.IpAddress ?? existing.IpAddress,
                Location = existing.Location
            };

            existing.RevokedAt = DateTime.UtcNow;
            existing.ReplacedByTokenId = newRefreshToken.Id;
            existing.LastUsedAt = DateTime.UtcNow;

            tokenRepo.Update(existing);
            await tokenRepo.AddAsync(newRefreshToken, cancellationToken);

            var roles = await _unitOfWork.Repository<UserRole>()
                .Query()
                .Where(ur => ur.UserId == user.Id && ur.Role != null)
                .Select(ur => ur.Role.Name)
                .ToListAsync(cancellationToken);

            string? driverStatus = null;
            if (user is Delivery driver)
            {
                driverStatus = driver.Status.ToString().ToUpperInvariant();
            }

            var accessToken = _jwtService.GenerateAccessToken(user, roles, driverStatus);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new TokenResponse(
                accessToken,
                newRawRefreshToken,
                DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                newRefreshToken.ExpiresAt);

            return Result<TokenResponse>.Success(response);
        }
    }
}
