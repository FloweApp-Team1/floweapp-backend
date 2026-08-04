using IdentityService.Domain.Entities;

namespace IdentityService.Common.Contracts
{
    public interface IUserRepository
    {
        // Returns the active, non-deleted account for the email, or null if none.
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
