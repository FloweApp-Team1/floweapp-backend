using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces
{
    public interface IUserRepository:IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct);
        Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
       
       
    }
}
