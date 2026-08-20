using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class ContentChangeLog : BaseEntity
    {
        public string EntityType { get; set; } = null!;   
        public Guid EntityId { get; set; }
        public string Action { get; set; } = null!;       
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Summary { get; set; }                
    }
}
