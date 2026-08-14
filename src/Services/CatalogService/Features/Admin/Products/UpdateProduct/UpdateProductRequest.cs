namespace CatalogService.Features.Admin.Products.UpdateProduct
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? Includes { get; set; }
        public decimal? Price { get; set; }
        public int? DiscountPercent { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        public List<Guid>? OccasionIds { get; set; }
        public List<IFormFile>? Images { get; set; }
        public string? StoreStock { get; set; }
    }
}
