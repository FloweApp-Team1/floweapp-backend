using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityService.Domain.Intefaces
{
    public interface IIdentityUnitOfWork
    {
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IAdminLoginAuditRepository AdminLoginAudits { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    }
}
