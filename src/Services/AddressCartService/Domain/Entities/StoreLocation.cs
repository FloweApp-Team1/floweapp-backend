namespace AddressCartService.Domain.Entities
{
    // Owned by Store (one-to-one) - not an aggregate of its own.
    public class StoreLocation
    {
        public string AddressLine { get; set; } = null!;
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
