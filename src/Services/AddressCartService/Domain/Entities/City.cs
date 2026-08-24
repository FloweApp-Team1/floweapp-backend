namespace AddressCartService.Domain.Entities
{
    public class City
    {
        public int Id { get; set; }
        public int GovernorateId { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;

        public Governorate Governorate { get; set; } = null!;
    }
}
