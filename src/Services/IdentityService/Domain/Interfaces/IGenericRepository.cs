using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
        Task AddAsync(T entity, CancellationToken ct);
    }
}
