using IdentityService.Domain.Entities;

namespace IdentityService.Common.Contracts
{
    public interface IUserRepository
    {
        // Read-only lookup of the active, non-deleted account for the email, or null if none.
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

        // Tracked lookup for updates (e.g. password reset), or null if none.
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task ResetPasswordAsync(User user, string newPasswordHash, CancellationToken cancellationToken = default);
    }
}
