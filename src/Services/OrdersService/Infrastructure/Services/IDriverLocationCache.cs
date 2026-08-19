using OrdersService.Features.DriverDelivery.CacheDto;

namespace OrdersService.Infrastructure.Services
{
    public interface IDriverLocationCache
    {
        Task<DriverLocationCacheDto?> GetAsync( Guid orderId, CancellationToken cancellationToken = default);

        Task SetAsync( DriverLocationCacheDto location, CancellationToken cancellationToken = default);

        Task DeleteAsync( Guid orderId, CancellationToken cancellationToken = default);
    }
}
