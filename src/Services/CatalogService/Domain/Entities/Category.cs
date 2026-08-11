using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Category : CatalogBaseEntity
    {
        public string Name { get; set; } = null!;
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
