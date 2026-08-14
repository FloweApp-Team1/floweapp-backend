namespace CatalogService.Domain.Entities
{
    public class ProductStoreStock : CatalogBaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public Guid StoreId { get; set; }
        public int Quantity { get; set; }
    }
}
