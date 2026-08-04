using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Intefaces
{
    public interface IAdminLoginAuditRepository : IGenericRepository<AdminLoginAudit>
    {
        Task<int> CountRecentFailedAttemptsByEmailAsync(string email, TimeSpan window, CancellationToken ct);
        Task<int> CountRecentFailedAttemptsByIpAsync(string ipAddress, TimeSpan window, CancellationToken ct);
    }

}

