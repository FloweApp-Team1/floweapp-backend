using System.Collections.Generic;

namespace AddressCartService.Domain.Entities
{
    public class Governorate
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;

        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
