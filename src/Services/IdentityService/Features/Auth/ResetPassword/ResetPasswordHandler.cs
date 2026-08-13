using IdentityService.Common.Contracts;
using Shared.Contracts;
using Shared.Responses;
using MediatR;

namespace IdentityService.Features.Auth.ResetPassword
{
    public sealed class ResetPasswordHandler
        : IRequestHandler<ResetPasswordCommand, ApiResponse<bool>>
    {
        private const string InvalidTokenMessage =
            "Invalid or expired reset token. Please request a new code.";

        private readonly IResetTokenService _resetTokens;
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordHandler(
            IResetTokenService resetTokens,
            IUserRepository users,
            IPasswordHasher passwordHasher)
        {
            _resetTokens = resetTokens;
            _users = users;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApiResponse<bool>> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var token = await _resetTokens.ValidateAsync(request.OtpToken);

            // Missing (already used / expired out of Redis) or past its lifetime.
            if (token is null || token.ExpiresAtUtc < DateTime.UtcNow)
                return ApiResponse<bool>.Fail(
                    InvalidTokenMessage,
                    StatusCodes.Status400BadRequest);

            var user = await _users.GetByEmailAsync(token.Email, cancellationToken);

            if (user is null)
                return ApiResponse<bool>.Fail(
                    InvalidTokenMessage,
                    StatusCodes.Status400BadRequest);

            var newHash = _passwordHasher.Hash(request.Password);

            // Sets the new hash and revokes all existing refresh tokens (kills every session).
            await _users.ResetPasswordAsync(user, newHash, cancellationToken);

            // Single-use: remove the token so it can't be replayed.
            await _resetTokens.InvalidateAsync(request.OtpToken);

            return ApiResponse.Success(true, "Password has been reset. Please log in.");
        }
    }
}
