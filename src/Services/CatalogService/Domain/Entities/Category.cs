using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product>? Products { get; set; }
    }
}
