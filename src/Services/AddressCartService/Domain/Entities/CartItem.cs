namespace AddressCartService.Domain.Entities
{
    public class CartItem : AddressCartBaseEntity
    {
        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        // External reference to CatalogService's Product - no navigation/FK across services.
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        // Unit price snapshot at the time this line was added/last changed, so the
        // handler can flag CartItemResponse.PriceChanged against the live Catalog price.
        public decimal PriceAtAdd { get; set; }
    }
}
