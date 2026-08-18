using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Product : CatalogBaseEntity
    {
        //public string Name { get; set; } = null!;
        //public string Description { get; set; } = null!;
        //public decimal Price { get; set; }
        //public int StockQuantity { get; set; }

        //public Guid CategoryId { get; set; }
        //public Category Category { get; set; } = null!;

        //public ICollection<ProductImage>? ProductImages { get; set; }
        //public ICollection<Occasion>? Occasions { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public decimal Price { get; set; }
        public int? DiscountPercent { get; set; }

        // Legacy aggregate stock (kept for backward compatibility with existing reads);
        // per-store levels live in ProductStoreStock.
        public int StockQuantity { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Short bullet list shown on the product page, e.g. ["Pink roses: 15", "White wrap"].
        // Persisted as its own table (see ProductInclude / ProductConfiguration).
        public ICollection<ProductInclude>? Includes { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; }

        // we don't need to have a direct collection of Occasions here, because we have a many-to-many relationship through ProductOccasion
        public ICollection<Occasion>? Occasions { get; set; }
        public ICollection<ProductOccasion>? ProductOccasions { get; set; }
        public ICollection<ProductStoreStock>? StoreStocks { get; set; }
        public ICollection<Discount> Discounts { get; set; } = [];

    }
}
