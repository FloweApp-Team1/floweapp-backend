using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Product : CatalogBaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductImage>? ProductImages { get; set; }
        public ICollection<Occasion>? Occasions { get; set; }
        public ICollection<Discount> Discounts { get; set; } = [];

    }
}
