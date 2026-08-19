namespace CatalogService.Domain.Entities
{
    public class ProductOccasion : CatalogBaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;


        public Guid OccasionId { get; set; }
        public Occasion Occasion { get; set; } = null!;
    }
}
