using IdentityService.Common.Contracts;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Infrastructure.Services.OTP
{
    public sealed class OtpHasher : IOtpHasher
    {
        private const string Secret =
            "FlowersAppPasswordResetOTP";

        public string Hash(string otp)
        {
            using var sha = SHA256.Create();

            var bytes = Encoding.UTF8.GetBytes(otp + Secret);

            var hash = sha.ComputeHash(bytes);

            return Convert.ToHexString(hash);
        }

        public bool Verify(string otp, string hash)
        {
            var computed = Hash(otp);

            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(computed),
                Convert.FromHexString(hash));
        }
    }
}
