using Shared.Domain;

namespace OrdersService.Domain.Entities
{
    public class OrdersBaseEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public Guid LastChangedBy { get; set; }

        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
