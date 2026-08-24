namespace AddressCartService.Infrastructure.Settings
{
    public class CatalogSettings
    {
        public const string SectionName = "CatalogService";
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 10;
    }
}
