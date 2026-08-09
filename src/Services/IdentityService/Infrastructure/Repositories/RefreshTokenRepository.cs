using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories
{
    public sealed class RefreshTokenRepository(AuthDbContext context)
       : GenericRepository<RefreshToken>(context), IRefreshTokenRepository
    {
          public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct)
            => await Context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ct);

      
        public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
            => await Context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);

        public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var activeTokens = await GetActiveByUserIdAsync(userId, ct);
            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }
    }
}
