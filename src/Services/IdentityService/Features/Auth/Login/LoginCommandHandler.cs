using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IPasswordHasher passwordHasher,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var email = request.Request.Email;

            var expectedRole = MapAppTypeToRole(request.AppType);

            var userProjection = await _unitOfWork.Repository<User>().Query()
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.Id,
                    u.PasswordHash,
                    u.IsActive,
                    RoleNames = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (userProjection == null || !userProjection.IsActive)
            {
                await RecordLoginAttemptAsync(email, false, cancellationToken);
                return Result<AuthResponse>.Failure("Invalid email or password");
            }

            if (!_passwordHasher.Verify(request.Request.Password, userProjection.PasswordHash))
            {
                await RecordLoginAttemptAsync(email, false, cancellationToken);
                return Result<AuthResponse>.Failure("Invalid email or password");
            }

            if (!userProjection.RoleNames.Contains(expectedRole, StringComparer.OrdinalIgnoreCase))
            {
                await RecordLoginAttemptAsync(email, false, cancellationToken);
                // The endpoint will map this specific string to 403 Forbidden.
                return Result<AuthResponse>.Failure("RoleAuthorizationFailed: Unauthorized access for this app type");
            }

            // Successful login
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userProjection.Id, cancellationToken);
            if (user == null)
            {
                return Result<AuthResponse>.Failure("User not found");
            }

            user.FcmToken = request.Request.FcmToken;
            _unitOfWork.Repository<User>().Update(user);

            var accessToken = _jwtService.GenerateAccessToken(user, new[] { expectedRole });

            var refreshTokenValue = _jwtService.GenerateRefreshTokenValue();
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var refreshTokenEntity = new IdentityService.Domain.Entities.RefreshToken
            {
                Token = refreshTokenValue,
                FamilyId = Guid.NewGuid(), // Treat as new session
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IpAddress = ipAddress,
                UserId = user.Id
            };

            await _unitOfWork.Repository<IdentityService.Domain.Entities.RefreshToken>().AddAsync(refreshTokenEntity, cancellationToken);

            await RecordLoginAttemptAsync(email, true, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AuthResponse(accessToken, refreshTokenValue, expectedRole);
            return Result<AuthResponse>.Success(response);
        }

        private string MapAppTypeToRole(string appType)
        {
            //mapping
            if (appType.Equals("Customer", StringComparison.OrdinalIgnoreCase)) return "Customer";
            if (appType.Equals("Driver", StringComparison.OrdinalIgnoreCase)) return "Driver";

            return appType;
        }

        private async Task RecordLoginAttemptAsync(string email, bool isSuccess, CancellationToken cancellationToken)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var attempt = new LoginAttempt
            {
                Email = email,
                AttemptedAt = DateTime.UtcNow,
                IsSuccess = isSuccess,
                IpAddress = ipAddress
            };

            await _unitOfWork.Repository<LoginAttempt>().AddAsync(attempt, cancellationToken);

            if (!isSuccess)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
