namespace OrdersService.Features.DriverDelivery.CacheDto
{
    public class DriverLocationCacheDto
    {
        public Guid OrderId { get; init; }
        public Guid DriverId { get; init; }

        public double Lat { get; init; }
        public double Lng { get; init; }

        public DateTime RecordedAt { get; init; }
    }
}
