namespace AddressCartService.Domain.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string PhoneCode { get; set; } = null!;
    }
}
