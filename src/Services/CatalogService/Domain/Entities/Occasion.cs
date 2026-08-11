using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Occasion : CatalogBaseEntity
    {
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
