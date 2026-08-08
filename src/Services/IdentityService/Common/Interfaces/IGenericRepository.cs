using IdentityService.Domain.Entities;
using System.Linq.Expressions;

namespace IdentityService.Common.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        IQueryable<T> Query();
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Remove(T entity);
        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }

  


}
