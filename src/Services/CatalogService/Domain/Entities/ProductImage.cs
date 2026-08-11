using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class ProductImage : CatalogBaseEntity
    {
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
