using CatalogService.Domain.Enums;
using Shared.Domain;

namespace CatalogService.Domain.Entities
{
    public class HomeLayoutSection : BaseEntity
    {
        public HomeSectionType Type { get; set; }
        public string? Title { get; set; }
        public short Order { get; set; }
        public bool IsEnabled { get; set; }
        public BaseSectionPayload Payload { get; set; } = null!;
    }
}
