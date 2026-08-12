using System.Text.Json.Serialization;
using CatalogService.Domain.Enums;

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
