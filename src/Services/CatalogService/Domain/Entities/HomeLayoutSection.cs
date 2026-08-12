using CatalogService.Domain.Enums;
using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class HomeLayoutSection : BaseEntity
    {
        public HomeSectionType type { get; set; }
        public string? title { get; set; }
        public short order { get; set; }
        public bool isEnabled { get; set; }
        public BaseSectionPayload Payload { get; set; } = null!;
    }
}
