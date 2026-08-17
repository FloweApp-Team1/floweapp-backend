namespace CatalogService.Domain.Entities
{
    public class HomeSectionProduct
    {
        public Guid HomeSectionId { get; set; }
        public HomeSection HomeSection { get; set; } = null!;
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
