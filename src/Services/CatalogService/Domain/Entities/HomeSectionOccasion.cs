namespace CatalogService.Domain.Entities
{
    public class HomeSectionOccasion
    {
        public Guid HomeSectionId { get; set; }
        public HomeSection HomeSection { get; set; } = null!;
        public Guid OccasionId { get; set; }
        public Occasion Occasion { get; set; } = null!;
    }
}
