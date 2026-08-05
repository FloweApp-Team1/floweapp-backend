

    using IdentityService.Common.Interfaces;
    using IdentityService.Common.Models;
    using IdentityService.Domain.Entities;
    using MediatR;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using RefreshTokenEntity = IdentityService.Domain.Entities.RefreshToken;

    namespace IdentityService.Features.Auth.RefreshTokens
    {
        
        public record RefreshTokenCommand(
            string RefreshTokenValue,
            string? IpAddress,
            string? DeviceName) : IRequest<Result<TokenResponse>>;

     
        public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
        {
            private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);   
            private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);     

            private readonly IUnitOfWork _unitOfWork;
            private readonly IJwtService _jwtService;

            public RefreshTokenHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
            {
                _unitOfWork = unitOfWork;
                _jwtService = jwtService;
            }

            public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
            {
                var tokenRepo = _unitOfWork.Repository<RefreshTokenEntity>();
                var existing = await tokenRepo.FirstOrDefaultAsync(t => t.Token == request.RefreshTokenValue, cancellationToken);

                if (existing is null)
                    return Result<TokenResponse>.Failure("Invalid refresh token.");

                // criteria 4: استخدام توكن اتلغى قبل كده (rotated/revoked) = إشارة اختراق
                // => نلغي الـ family كلها (كل الأجهزة) وناخد المستخدم يعمل login تاني في كل مكان
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

             
                var newRefreshToken = new RefreshTokenEntity
                {
                    Id = Guid.NewGuid(),
                    Token = _jwtService.GenerateRefreshTokenValue(),
                    FamilyId = existing.FamilyId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
                    DeviceName = request.DeviceName ?? existing.DeviceName,
                    IpAddress = request.IpAddress ?? existing.IpAddress,
                    Location = existing.Location
                };

                existing.RevokedAt = DateTime.UtcNow;
                existing.ReplacedByTokenId = newRefreshToken.Id;
                existing.LastUsedAt = DateTime.UtcNow;

                tokenRepo.Update(existing);
                await tokenRepo.AddAsync(newRefreshToken, cancellationToken);

                var roles = user.UserRoles?
                    .Where(ur => ur.Role != null)
                    .Select(ur => ur.Role.Name) ?? Enumerable.Empty<string>();

                var accessToken = _jwtService.GenerateAccessToken(user, roles);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = new TokenResponse(
                    accessToken,
                    newRefreshToken.Token,
                    DateTime.UtcNow.Add(AccessTokenLifetime),
                    newRefreshToken.ExpiresAt);

                return Result<TokenResponse>.Success(response);
            }
        }

    
        }
    


