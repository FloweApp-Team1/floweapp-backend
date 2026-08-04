using IdentityService.Domain.Entities;
using IdentityService.Domain.Intefaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories
{
    public sealed class AdminLoginAuditRepository(AuthDbContext context)
         : GenericRepository<AdminLoginAudit>(context), IAdminLoginAuditRepository
    {
     
        public async Task<int> CountRecentFailedAttemptsByEmailAsync(string email, TimeSpan window, CancellationToken ct)
        {
            var since = DateTime.UtcNow.Subtract(window);
            return await Context.AdminLoginAudits
                .CountAsync(a => a.Email == email && !a.IsSuccess && a.AttemptedAt >= since, ct);
        }

        public async Task<int> CountRecentFailedAttemptsByIpAsync(string ipAddress, TimeSpan window, CancellationToken ct)
        {
            var since = DateTime.UtcNow.Subtract(window);
            return await Context.AdminLoginAudits
                .CountAsync(a => a.IpAddress == ipAddress && !a.IsSuccess && a.AttemptedAt >= since, ct);
        }
    }
}
