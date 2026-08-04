using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Intefaces
{
    public interface IUserRepository:IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct);
        Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken ct);
        Task AssignRoleAsync(Guid userId, Guid roleId, CancellationToken ct);
    }
}
