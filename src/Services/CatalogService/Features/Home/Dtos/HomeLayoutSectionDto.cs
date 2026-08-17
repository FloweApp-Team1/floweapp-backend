using System.Text.Json.Serialization;

namespace CatalogService.Features.Home.Dtos
{
    public class HomeLayoutSectionDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string? Title { get; set; }
        public short Order { get; set; }
        public bool IsEnabled { get; set; }
        public BaseSectionPayloadDto Payload { get; set; } = null!;
    }

    // Required for System.Text.Json to serialize the runtime type (BannerPayloadDto, etc.)
    // rather than the declared type (BaseSectionPayloadDto), which would produce {}.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ProductRailPayloadDto), "product_rail")]
    [JsonDerivedType(typeof(OccasionRailPayloadDto), "occasion_rail")]
    [JsonDerivedType(typeof(CategoryRailPayloadDto), "category_rail")]
    [JsonDerivedType(typeof(BannerPayloadDto), "banner")]
    public abstract class BaseSectionPayloadDto
    {
    }

    public class ProductRailPayloadDto : BaseSectionPayloadDto
    {
        public List<ProductItemDto> Items { get; set; } = new();
        public string? ViewAllAction { get; set; }
    }

    public class ProductItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public decimal Price { get; set; }
    }

    public class OccasionRailPayloadDto : BaseSectionPayloadDto
    {
        public List<OccasionItemDto> Items { get; set; } = new();
        public string? ViewAllAction { get; set; }
    }

    public class OccasionItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }

    public class CategoryRailPayloadDto : BaseSectionPayloadDto
    {
        public List<CategoryItemDto> Items { get; set; } = new();
        public string? ViewAllAction { get; set; }
    }

    public class CategoryItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string IconUrl { get; set; } = null!;
    }

    public class BannerPayloadDto : BaseSectionPayloadDto
    {
        public string ImageUrl { get; set; } = null!;
        public string ClickAction { get; set; } = null!;
    }
}
