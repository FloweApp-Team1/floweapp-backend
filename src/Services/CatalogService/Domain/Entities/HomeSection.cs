using CatalogService.Domain.Enums;

namespace CatalogService.Domain.Entities
{
    public class HomeSection : CatalogBaseEntity
    {
        public HomeSectionKind Type { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool Enabled { get; set; } = true;

        // Banner-only fields
        public string? ViewAllDeeplink { get; set; }
        public string? BannerImageUrl { get; set; }
        public string? BannerDeeplink { get; set; }

        // Rail-only fields
        public ProductSelectionRule? ProductSelectionRule { get; set; }

        public ICollection<HomeSectionCategory>? SectionCategories { get; set; }
        public ICollection<HomeSectionOccasion>? SectionOccasions { get; set; }
        public ICollection<HomeSectionProduct>? SectionProducts { get; set; }
    }
}
