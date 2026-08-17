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
    }
}
