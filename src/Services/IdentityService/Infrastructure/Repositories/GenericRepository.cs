using IdentityService.Common.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdentityService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly AuthDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AuthDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

        
        public IQueryable<T> Query() => _dbSet.AsQueryable();

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity)
        {
        
            entity.IsDeleted = true;
            _dbSet.Update(entity);
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
            => _dbSet.Any(predicate);
    }
}
