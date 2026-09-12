using AddressCartService.Domain.Enums;

namespace AddressCartService.Domain.Entities
{
    public class Store : AddressCartBaseEntity
    {
        public string Name { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? ImageUrl { get; set; }

        public StoreLocation Location { get; set; } = null!;
        public CoverageArea CoverageArea { get; set; } = null!;

        public StoreStatusEnum Status { get; set; } = StoreStatusEnum.Active;

        public ICollection<Address>? Addresses { get; set; }
    }
}
