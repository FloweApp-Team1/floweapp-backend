using IdentityService.Common.Contracts;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Services
{
    // PBKDF2 (SHA-256) password hasher. Stores "iterations.salt.key" so the salt and
    // work factor travel with the hash and can be verified/upgraded later.
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            var key = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, Algorithm, KeySize);

            return string.Join(
                '.',
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key));
        }

        public bool Verify(string password, string hash)
        {
            var parts = hash.Split('.', 3);

            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
                return false;

            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var attempted = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterations, Algorithm, key.Length);

            return CryptographicOperations.FixedTimeEquals(attempted, key);
        }
    }
}
