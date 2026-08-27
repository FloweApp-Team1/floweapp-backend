using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Domain;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CatalogDbContext _context;
        private IDbContextTransaction? _transaction;

        private readonly Dictionary<Type, object> _repositories = new();

        private int _depth;

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

      
        public async Task<Result> ExecuteAsync(
            Func<Task<Result>> action, CancellationToken cancellationToken = default)
        {
            Result? captured = null;

            await RunWithDepthTrackingAsync(async () =>
            {
                captured = await action();
                return (captured.IsSuccess, captured.Error);
            }, cancellationToken);

            return captured!;
        }

        public async Task<Result<T>> ExecuteAsync<T>(
            Func<Task<Result<T>>> action, CancellationToken cancellationToken = default)
        {
            Result<T>? captured = null;

            await RunWithDepthTrackingAsync(async () =>
            {
                captured = await action();
                return (captured.IsSuccess, captured.Error);
            }, cancellationToken);

            return captured!;
        }

        private async Task<(bool IsSuccess, Error Error)> RunWithDepthTrackingAsync(
            Func<Task<(bool IsSuccess, Error Error)>> action, CancellationToken cancellationToken)
        {
            var isOutermost = _depth == 0;

            if (isOutermost)
                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            _depth++;

            try
            {
                var (isSuccess, error) = await action();

                _depth--;

                if (_depth == 0)
                {
                    if (isSuccess)
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                        await _transaction!.CommitAsync(cancellationToken);
                    }
                    else
                    {
                        await _transaction!.RollbackAsync(cancellationToken);
                    }

                    await _transaction!.DisposeAsync();
                    _transaction = null;
                }

                return (isSuccess, error);
            }
            catch
            {
                _depth--;

                if (_depth == 0 && _transaction is not null)
                {
                    await _transaction.RollbackAsync(cancellationToken);
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }

                throw;
            }
        }
    }
}
