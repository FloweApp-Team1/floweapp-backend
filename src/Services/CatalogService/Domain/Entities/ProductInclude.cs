namespace CatalogService.Domain.Entities
{
    public class ProductInclude : CatalogBaseEntity
    {
        public string Name { get; set; } = null!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
