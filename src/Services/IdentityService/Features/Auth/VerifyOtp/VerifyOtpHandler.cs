using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using MediatR;

namespace IdentityService.Features.Auth.VerifyOtp
{
    public sealed class VerifyOtpHandler
    : IRequestHandler<
        VerifyOtpCommand,
        ApiResponse<VerifyOtpResponse>>
    {
        private readonly IUserRepository _users;
        private readonly IOtpService _otpService;

        public VerifyOtpHandler(
            IUserRepository users,
            IOtpService otpService)
        {
            _users = users;
            _otpService = otpService;
        }

        public async Task<ApiResponse<VerifyOtpResponse>> Handle(
            VerifyOtpCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _users.FindByEmailAsync(request.Email, cancellationToken);

            // Unknown email is treated exactly like a wrong code, so we never
            // reveal whether the account exists.
            if (user is null)
            {
                return ApiResponse<VerifyOtpResponse>.Fail(
                    "Invalid verification code.");
            }

            var result = await _otpService.VerifyAsync(
                user.Email,
                request.Otp);

            if (!result.Success)
            {
                return ApiResponse<VerifyOtpResponse>.Fail(
                    result.ErrorMessage!,
                    StatusCodes.Status400BadRequest);
            }

            return ApiResponse.Success(
                new VerifyOtpResponse(
                    result.ResetToken!,
                    result.ExpiresInMinutes),
                "OTP verified");
        }
    }
}
