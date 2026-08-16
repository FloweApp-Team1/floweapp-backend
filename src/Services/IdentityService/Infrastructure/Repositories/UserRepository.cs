using IdentityService.Common.Contracts;
using Shared.Contracts;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace IdentityService.Infrastructure.Repositories
{
    // Keeps AuthDbContext behind an interface so feature handlers never touch EF directly.
    public sealed class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;

        public UserRepository(AuthDbContext db)
        {
            _db = db;
        }

        public Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Email == email && u.IsActive ,
                    cancellationToken);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Email == email && u.IsActive ,
                    cancellationToken);
        }

        public async Task ResetPasswordAsync(
            User user,
            string newPasswordHash,
            CancellationToken cancellationToken = default)
        {
            _db.Users.Attach(user);
            user.PasswordHash = newPasswordHash;

            // Invalidate every active session for the account.
            var activeTokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
                token.RevokedAt = DateTime.UtcNow;

            // Single SaveChanges -> password update and revocations commit atomically.
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
