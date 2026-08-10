using Shared.Contracts;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Services
{
    // The one password hashing implementation for the service.
    //
    // New hashes are BCrypt with work factor 12. Verify also accepts the legacy
    // PBKDF2 format ("iterations.salt.key") that accounts registered before this
    // consolidation were stored with, so no existing user is locked out; those
    // rows move to BCrypt the next time their password is set.
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        // Legacy PBKDF2 parameters - verification only, never used to hash.
        private static readonly HashAlgorithmName LegacyAlgorithm = HashAlgorithmName.SHA256;

        public string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return false;

            return IsLegacyPbkdf2(hash)
                ? VerifyLegacyPbkdf2(password, hash)
                : VerifyBCrypt(password, hash);
        }

        // BCrypt hashes always start with "$2"; the legacy format is "iterations.salt.key".
        private static bool IsLegacyPbkdf2(string hash) =>
            !hash.StartsWith('$') && hash.Split('.').Length == 3;

        private static bool VerifyBCrypt(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Malformed or placeholder hash (e.g. the timing-attack dummy) - not a match.
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool VerifyLegacyPbkdf2(string password, string hash)
        {
            var parts = hash.Split('.', 3);

            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[1]);
                var key = Convert.FromBase64String(parts[2]);

                var attempted = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, iterations, LegacyAlgorithm, key.Length);

                return CryptographicOperations.FixedTimeEquals(attempted, key);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
