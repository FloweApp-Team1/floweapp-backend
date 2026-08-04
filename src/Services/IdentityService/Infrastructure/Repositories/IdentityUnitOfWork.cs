using IdentityService.Domain.Intefaces;

namespace IdentityService.Infrastructure.Repositories
{
    public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
    {
        private readonly AuthDbContext _context;

        public IdentityUnitOfWork(
            AuthDbContext context,
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            IAdminLoginAuditRepository adminLoginAudits)
        {
            _context = context;
            Users = users;
            RefreshTokens = refreshTokens;
            AdminLoginAudits = adminLoginAudits;
        }

        public IUserRepository Users { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public IAdminLoginAuditRepository AdminLoginAudits { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);
    }
}
