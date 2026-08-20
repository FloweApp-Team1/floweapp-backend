using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PaymentService.Infrastructure.Persistence;
using Shared.Domain;
using Shared.Interfaces;
using System.Linq.Expressions;

namespace PaymentService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly PaymentDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(PaymentDbContext context)
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

        public void SaveInclude(T entity, params string[] includedProperties)
        {

            var localEntity = _dbSet.Local.FirstOrDefault(e => EqualityComparer<Guid>.Default.Equals(e.Id, entity.Id));

            EntityEntry<T> entry;

            if (localEntity == null)
            {
                _dbSet.Attach(entity);
                entry = _context.Entry(entity);
            }
            else
            {
                entry = _context.Entry(localEntity);
                entry.CurrentValues.SetValues(entity);
            }

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                property.IsModified = includedProperties.Contains(property.Metadata.Name);
            }
        }
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
