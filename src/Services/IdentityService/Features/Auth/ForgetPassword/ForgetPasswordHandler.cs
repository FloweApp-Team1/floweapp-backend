using IdentityService.Common.Contracts;
using Shared.Contracts;
using Shared.Responses;
using MediatR;

namespace IdentityService.Features.Auth.ForgetPassword
{
    public sealed class ForgetPasswordHandler : IRequestHandler<ForgetPasswordCommand, ApiResponse<bool>>
    {
        // Same message whether or not the email exists, so the response never
        // confirms/denies that an account is registered (acceptance criteria).
        private const string NeutralMessage =
            "If this email is registered, a code has been sent";

        private readonly IUserRepository _users;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgetPasswordHandler> _logger;

        public ForgetPasswordHandler(
            IUserRepository users,
            IOtpService otpService,
            IEmailService emailService,
            ILogger<ForgetPasswordHandler> logger)
        {
            _users = users;
            _otpService = otpService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(
            ForgetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.email.Trim();

            var user = await _users.FindByEmailAsync(email, cancellationToken);

            // Unknown / inactive / deleted: return the neutral response without generating an OTP.
            if (user is null)
                return ApiResponse.Success(true, NeutralMessage);

            var otp = await _otpService.GenerateAsync(user.Id, user.Email);

            // Resend requested before the 30s cooldown elapsed: still neutral success.
            if (otp is null)
                return ApiResponse.Success(true, NeutralMessage);

            DispatchOtpEmail(user.Email, otp);

            return ApiResponse.Success(true, NeutralMessage);
        }

        private void DispatchOtpEmail(string email, string otp)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendPasswordResetOtpAsync(email, otp);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset OTP email.");
                }
            });
        }
    }
}
