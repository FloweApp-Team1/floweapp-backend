using OrdersService.Infrastructure.Persistence;
using Shared.Domain;
using Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace OrdersService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly OrdersDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(OrdersDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

        public IQueryable<T> Query() => _dbSet.AsQueryable();

        public IQueryable<T> GetAll(Expression<Func<T, bool>>? expression = null)
        {
            if (expression is not null)
                return _dbSet.Where(expression);
            return _dbSet;
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity)
        {
            entity.IsDeleted = true;
            _dbSet.Update(entity);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => _dbSet.AnyAsync(predicate);
    }
}
