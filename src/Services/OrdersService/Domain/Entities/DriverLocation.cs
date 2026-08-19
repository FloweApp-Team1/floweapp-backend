namespace OrdersService.Domain.Entities
{
    public class DriverLocation : OrdersBaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // External reference to IdentityService's driver User - no navigation/FK across services.
        public Guid DriverId { get; set; }

        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime RecordedAt { get; set; }

        // When this position was last broadcast as a DriverLocationUpdatedEvent. Pings
        // arrive far more often than the customer needs a push, so this is what the
        // "meaningful update" throttle in UpdateDriverLocationHandler measures its interval
        // against - RecordedAt cannot serve that role because every ping overwrites it.
        public DateTime? LastBroadcastAt { get; set; }
    }
}