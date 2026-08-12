using System.Text.Json.Serialization;

namespace CatalogService.Domain.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ProductRailPayload), "product_rail")]
    [JsonDerivedType(typeof(OccasionRailPayload), "occasion_rail")]
    [JsonDerivedType(typeof(CategoryRailPayload), "category_rail")]
    [JsonDerivedType(typeof(BannerPayload), "banner")]
    public abstract class BaseSectionPayload
    {
    }

    public class ProductRailPayload : BaseSectionPayload
    {
        public int Count { get; set; } = 10;
    }

    public class OccasionRailPayload : BaseSectionPayload
    {
        public int Count { get; set; } = 10;
    }

    public class CategoryRailPayload : BaseSectionPayload
    {
        public int Count { get; set; } = 5;
    }

    public class BannerPayload : BaseSectionPayload
    {
        public string ImageUrl { get; set; } = null!;
        public string ClickAction { get; set; } = null!;
    }
}
