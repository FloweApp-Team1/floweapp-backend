using Org.BouncyCastle.Crypto.Generators;

namespace IdentityService.Common.Security
{
    public sealed class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
