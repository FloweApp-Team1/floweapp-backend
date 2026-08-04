using global::IdentityService.Common.Responses;
using MediatR;

namespace IdentityService.Features.Auth.VerifyOtp
{
    public sealed record VerifyOtpCommand( string Email, string Otp) : IRequest<ApiResponse<VerifyOtpResponse>>;
}
