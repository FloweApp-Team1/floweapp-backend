using Shared.Domain;
using System.Linq.Expressions;

namespace Shared.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        IQueryable<T> GetAll(Expression<Func<T, bool>>? expression = null);

        IQueryable<T> Query();
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Remove(T entity);
        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }

  


}
