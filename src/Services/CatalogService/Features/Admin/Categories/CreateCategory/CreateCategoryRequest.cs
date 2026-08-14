namespace CatalogService.Features.Admin.Categories.CreateCategory
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = null!;
        public int Order { get; set; }
        public IFormFile? Icon { get; set; }
    }
}
