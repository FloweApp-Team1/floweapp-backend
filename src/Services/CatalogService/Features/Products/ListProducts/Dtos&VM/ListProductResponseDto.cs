namespace CatalogService.Features.Products.ListProducts.Dtos
{
    public class ListProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal OrignalPrice { get; set; } 
        public decimal DiscountPrice { get; set; } 
        public decimal DiscountPercentage { get; set; } 
        public bool IsOutOfStock { get; set; } 

        public List<ProductImageDto> ProductImages { get; set; } = [];
    }

    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
}
