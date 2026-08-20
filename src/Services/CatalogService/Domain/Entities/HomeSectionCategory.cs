namespace CatalogService.Domain.Entities
{
    public class HomeSectionCategory
    {
        public Guid HomeSectionId { get; set; }
        public HomeSection HomeSection { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
