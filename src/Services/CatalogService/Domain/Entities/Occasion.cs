using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Occasion : CatalogBaseEntity
    {
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        // we don't need to have a direct collection of Products here, because we have a many-to-many relationship through ProductOccasion
        public ICollection<Product>? Products { get; set; }
        public ICollection<ProductOccasion>? ProductOccasions { get; set; }
    }
}
