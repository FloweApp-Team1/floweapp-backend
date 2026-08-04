using IdentityService.Common.Contracts;
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
                    u => u.Email == email && u.IsActive && !u.IsDeleted,
                    cancellationToken);
        }
    }
}
