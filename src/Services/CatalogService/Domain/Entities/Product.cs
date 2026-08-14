using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Product : CatalogBaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductInclude>? Includes { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; }

        // we don't need to have a direct collection of Occasions here, because we have a many-to-many relationship through ProductOccasion
        public ICollection<Occasion>? Occasions { get; set; }
        public ICollection<ProductOccasion>? ProductOccasions { get; set; }
    }
}
