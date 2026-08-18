namespace CatalogService.Features.Products.ListProducts.Dtos_VM
{
    public class ListProductResponseVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal OrignalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public bool IsOutOfStock { get; set; }

        public List<ProductImageVM> ProductImages { get; set; } = [];
    }

    public class ProductImageVM
    {
        public string Id { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
}
