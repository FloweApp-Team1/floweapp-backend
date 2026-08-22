using Shared.Domain;

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
    }
}
