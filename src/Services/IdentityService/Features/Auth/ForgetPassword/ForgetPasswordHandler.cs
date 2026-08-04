using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
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

        public ForgetPasswordHandler(
            IUserRepository users,
            IOtpService otpService,
            IEmailService emailService)
        {
            _users = users;
            _otpService = otpService;
            _emailService = emailService;
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

            await _emailService.SendPasswordResetOtpAsync(user.Email, otp);

            return ApiResponse.Success(true, NeutralMessage);
        }
    }
}
