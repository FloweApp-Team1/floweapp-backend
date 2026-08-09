using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories
{  
        public sealed class UserRepository(AuthDbContext context):GenericRepository<User>(context), IUserRepository
        {
            
                public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
                    => await Context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == email , ct);

                public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct)
                    => await Context.Users
                        .AsNoTracking()
                        .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                        .FirstOrDefaultAsync(u => u.Email == email , ct);

                public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct)
                    => await Context.Users
                        .AsNoTracking()
                        .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                        .FirstOrDefaultAsync(u => u.Id == id , ct);

              
                public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
                    => await Context.Users.AnyAsync(u => u.Email == email , ct);

            
            }
}
  