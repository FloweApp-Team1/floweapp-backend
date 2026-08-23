namespace AddressCartService.Infrastructure.Settings
{
    public class CatalogSettings
    {
        public const string SectionName = "CatalogService";
        public string BaseUrl { get; set; } = "http://localhost:5194";
        public int TimeoutSeconds { get; set; } = 10;
    }
}
