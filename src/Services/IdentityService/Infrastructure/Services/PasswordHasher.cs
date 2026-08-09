using IdentityService.Common.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _hasher = new();

        public string Hash(string password) =>
            _hasher.HashPassword(new User(), password);

        public bool Verify(string password, string hash) =>
            _hasher.VerifyHashedPassword(new User(), hash, password) != PasswordVerificationResult.Failed;
    }
}
