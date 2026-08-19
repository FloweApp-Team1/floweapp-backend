using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class CatalogBaseEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public Guid LastChangedBy { get; set; }
    }
}
