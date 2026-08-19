using global::Shared.Contracts;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Services.OTP
{
    public sealed class OtpGenerator : IOtpGenerator
    {
        private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public string Generate()
        {
            Span<char> otp = stackalloc char[6];

            Span<byte> bytes = stackalloc byte[6];

            RandomNumberGenerator.Fill(bytes);

            for (var i = 0; i < otp.Length; i++)
            {
                otp[i] = Characters[
                    bytes[i] % Characters.Length];
            }

            return new string(otp);
        }
    }
}
