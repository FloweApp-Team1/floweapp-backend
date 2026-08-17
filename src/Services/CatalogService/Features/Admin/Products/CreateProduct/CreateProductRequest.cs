namespace CatalogService.Features.Admin.Products.CreateProduct
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string>? Includes { get; set; }
        public decimal Price { get; set; }
        public int? DiscountPercent { get; set; }
        public List<Guid> CategoryIds { get; set; } = new();
        public List<Guid>? OccasionIds { get; set; }
        public List<IFormFile>? Images { get; set; }
        public string? StoreStock { get; set; }
    }
}
