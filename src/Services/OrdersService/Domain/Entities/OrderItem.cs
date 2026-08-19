namespace OrdersService.Domain.Entities
{
    public class OrderItem : OrdersBaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // External reference to CatalogService's Product - no navigation/FK across services.
        public Guid ProductId { get; set; }

        // Snapshots at order time, since the live Catalog product can change/disappear later.
        public string ProductName { get; set; } = null!;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
