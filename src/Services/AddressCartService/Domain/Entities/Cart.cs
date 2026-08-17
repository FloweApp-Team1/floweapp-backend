namespace AddressCartService.Domain.Entities
{
    public class Cart : AddressCartBaseEntity
    {
        // External reference to IdentityService's User - no navigation/FK across services.
        public Guid UserId { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Optimistic concurrency token: guards against two concurrent add/update-item
        // requests for the same cart silently overwriting one another.
        public byte[]? RowVersion { get; set; }
    }
}
