namespace AddressCartService.Domain.Entities
{
    public class Address : AddressCartBaseEntity
    {
        public Guid UserId { get; set; }

        public string RecipientName { get; set; } = null!;
        public string RecipientPhone { get; set; } = null!;
        public string AddressLine { get; set; } = null!;
        public int GovernorateId { get; set; }
        public int CityId { get; set; }
        public Governorate? Governorate { get; set; }
        public City? City { get; set; }
        public string Area { get; set; } = null!;
        public string? Label { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public bool IsDefault { get; set; }

        // Resolved serving store. A genuine local FK/navigation - unlike the cross-service
        // references above, Store is owned by this same service.
        public Guid? StoreId { get; set; }
        public Store? Store { get; set; }

        public bool IsServiceable { get; set; }
    }
}
