using Shared.Domain;
using Shared.Results;

namespace Shared.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(
      CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(
            CancellationToken cancellationToken = default);
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        
        Task<Result> ExecuteAsync(Func<Task<Result>> action, CancellationToken cancellationToken = default);
        Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> action, CancellationToken cancellationToken = default);
    }
}
