using IdentityService.Common.Contracts;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Infrastructure.Services.OTP
{
    public sealed class OtpHasher : IOtpHasher
    {
        private readonly byte[] _pepperKey;

        public OtpHasher(OtpSettings settings)
        {
            _pepperKey = Encoding.UTF8.GetBytes(settings.PepperSecret);
        }

        public string Hash(string otp)
        {
            var hash = HMACSHA256.HashData(_pepperKey, Encoding.UTF8.GetBytes(otp));

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
