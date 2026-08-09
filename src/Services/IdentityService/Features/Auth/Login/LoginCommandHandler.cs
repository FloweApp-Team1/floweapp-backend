using IdentityService.Common.Contracts;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var email = request.Request.Email;
            // var expectedRole = MapAppTypeToRole(request.AppType);

            var userProjection = await _unitOfWork.Repository<User>().Query()
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.PhoneNumber,
                    u.FullName,
                    u.CreatedAt,
                    u.Gender,
                    u.NotificationStatus,
                    u.PasswordHash,
                    u.IsActive,
                    RoleNames = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (userProjection == null || !userProjection.IsActive)
            {
                await RecordLoginAttemptAsync(email, false, request.IpAddress, cancellationToken);
                return Result<AuthResponse>.Failure("Invalid email or password");
            }

            if (!_passwordHasher.Verify(request.Request.Password, userProjection.PasswordHash))
            {
                await RecordLoginAttemptAsync(email, false, request.IpAddress, cancellationToken);
                return Result<AuthResponse>.Failure("Invalid email or password");
            }

            /*
            if (!userProjection!.RoleNames.Contains(expectedRole, StringComparer.OrdinalIgnoreCase))
            {
                await RecordLoginAttemptAsync(email, false, request.IpAddress, cancellationToken);
                //403 Forbidden.
                return Result<AuthResponse>.Failure("RoleAuthorizationFailed: Unauthorized access for this app type");
            }
            */


            await _unitOfWork.Repository<User>().Query()
                .Where(u => u.Id == userProjection.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.FcmToken, request.Request.FcmToken), cancellationToken);

            var dummyUserForJwt = new User 
            { 
                Id = userProjection.Id, 
                Email = userProjection.Email, 
                FullName = userProjection.FullName 
            };

            var accessToken = _jwtService.GenerateAccessToken(dummyUserForJwt, userProjection.RoleNames); // Use all roles for JWT claims
            var refreshTokenValue = _jwtService.GenerateRefreshTokenValue();
            
            // Must use the same hashing as IJwtService: /auth/refresh-token looks the
            // token up by its hex-encoded SHA-256. Hashing it locally to base64 here
            // produced a value that lookup could never match, so every refresh of a
            // token issued by /auth/login failed with "Invalid refresh token".
            var hashedToken = _jwtService.HashRefreshTokenValue(refreshTokenValue);

            var refreshTokenEntity = new IdentityService.Domain.Entities.RefreshToken
            {
                Token = hashedToken,
                FamilyId = Guid.NewGuid(), // Treat as new session
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IpAddress = request.IpAddress,
                UserId = userProjection.Id
            };

            await _unitOfWork.Repository<IdentityService.Domain.Entities.RefreshToken>().AddAsync(refreshTokenEntity, cancellationToken);
            await RecordLoginAttemptAsync(email, true, request.IpAddress, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = new UserDto(
                Id: userProjection.Id,
                Email: userProjection.Email,
                Phone: userProjection.PhoneNumber,
                Name: userProjection.FullName,
                Roles: userProjection.RoleNames,
                CreatedAt: userProjection.CreatedAt,
                UpdatedAt: userProjection.CreatedAt, // Binding UpdatedAt to CreatedAt
                Gender: userProjection.Gender.ToString().ToUpper(),
                NotificationStatus: userProjection.NotificationStatus.ToString().ToUpper()
            );

            var response = new AuthResponse(userDto, accessToken, refreshTokenValue);
            return Result<AuthResponse>.Success(response);
        }

        /*
        private string MapAppTypeToRole(string appType)
        {
            //mapping
            if (appType.Equals("Customer", StringComparison.OrdinalIgnoreCase)) return "Customer";
            if (appType.Equals("Driver", StringComparison.OrdinalIgnoreCase)) return "Driver";

            return appType;
        }
        */

        private async Task RecordLoginAttemptAsync(string email, bool isSuccess, string? ipAddress, CancellationToken cancellationToken)
        {
            var attempt = new LoginAttempt
            {
                Email = email,
                AttemptedAt = DateTime.UtcNow,
                IsSuccess = isSuccess,
                IpAddress = ipAddress ?? "Unknown"
            };

            await _unitOfWork.Repository<LoginAttempt>().AddAsync(attempt, cancellationToken);

            if (!isSuccess)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
