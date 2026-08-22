using CatalogService.Infrastructure.Persistence;
using Shared.Domain;
using Shared.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatalogService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CatalogDbContext _context;
        private IDbContextTransaction? _transaction;

        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(CatalogDbContext context)
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

        public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database
                .BeginTransactionAsync(cancellationToken);
        }
        public async Task RollbackTransactionAsync(
       CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return;

            await _transaction.RollbackAsync(cancellationToken);

            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                throw new InvalidOperationException(
                    "Transaction has not been started.");

            await _transaction.CommitAsync(cancellationToken);

            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }
}
