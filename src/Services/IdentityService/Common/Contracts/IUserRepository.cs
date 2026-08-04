using IdentityService.Domain.Entities;

namespace IdentityService.Common.Contracts
{
    public interface IUserRepository
    {
        // Read-only lookup of the active, non-deleted account for the email, or null if none.
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

        // Tracked lookup for updates (e.g. password reset), or null if none.
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        // Sets the new password hash and revokes every active refresh token for the user,
        // committed as a single unit of work.
        Task ResetPasswordAsync(User user, string newPasswordHash, CancellationToken cancellationToken = default);
    }
}
