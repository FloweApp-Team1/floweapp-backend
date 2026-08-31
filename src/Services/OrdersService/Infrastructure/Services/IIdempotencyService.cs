namespace OrdersService.Infrastructure.Services
{
   
    public interface IIdempotencyService
    {
        public sealed record IdempotencyReservation<TResponse>(
         bool Acquired,
         bool AlreadyCompleted,
         TResponse? CachedResult) where TResponse : class;

        public interface IIdempotencyService
        {
            Task<IdempotencyReservation<TResponse>> TryReserveAsync<TResponse>(
                Guid userId, string idempotencyKey, CancellationToken cancellationToken) where TResponse : class;

            Task CompleteReservationAsync<TResponse>(
                Guid userId, string idempotencyKey, TResponse response, CancellationToken cancellationToken)
                where TResponse : class;

            Task ReleaseReservationAsync(Guid userId, string idempotencyKey, CancellationToken cancellationToken);
        }
    }
}
