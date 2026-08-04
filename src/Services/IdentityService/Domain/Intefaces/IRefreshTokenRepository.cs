using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Intefaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct);
        Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
        Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct);
    }
}
