using IdentityService.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Services
{
    public class PasswordHasher:IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

        public bool Verify(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
