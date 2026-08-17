using Shared.Domain;

namespace AddressCartService.Domain.Entities
{
    public class AddressCartBaseEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public Guid LastChangedBy { get; set; }
    }
}
