using IdentityService.Common.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuthDbContext _context;

      
        
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AuthDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
                _repositories[type] = new GenericRepository<T>(_context);

            return (IGenericRepository<T>)_repositories[type];
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }
}
