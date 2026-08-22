namespace OrdersService.Domain.Entities
{
    // Snapshot of the delivery address at order time, preserved even if the source saved
    // address (owned by AddressCartService) is later edited or deleted.
    public class OrderAddressSnapshot : OrdersBaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string RecipientName { get; set; } = null!;
        public string RecipientPhone { get; set; } = null!;
        public string AddressLine { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Area { get; set; } = null!;
        public double? Lat { get; set; }
        public double? Lng { get; set; }
    }
}
