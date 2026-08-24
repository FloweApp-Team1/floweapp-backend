namespace AddressCartService.Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtAdd { get; set; }
    }
}
