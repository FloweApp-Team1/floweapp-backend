namespace OrdersService.Infrastructure.Services
{
   
    public interface IIdempotencyService
    {
        Task<TResponse?> GetCachedResponseAsync<TResponse>(
            Guid userId, string idempotencyKey, CancellationToken cancellationToken) where TResponse : class;

        Task StoreResponseAsync<TResponse>(
            Guid userId, string idempotencyKey, TResponse response, CancellationToken cancellationToken)
            where TResponse : class;
    }
}
