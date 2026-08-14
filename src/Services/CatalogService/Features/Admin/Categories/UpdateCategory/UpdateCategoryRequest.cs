namespace CatalogService.Features.Admin.Categories.UpdateCategory
{
    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }
        public int? Order { get; set; }
        public IFormFile? Icon { get; set; }
    }
}
