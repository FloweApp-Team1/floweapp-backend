using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class Occasion : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product>? Products { get; set; }
    }
}
