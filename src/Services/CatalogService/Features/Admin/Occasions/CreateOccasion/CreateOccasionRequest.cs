namespace CatalogService.Features.Admin.Occasions.CreateOccasion
{
    public class CreateOccasionRequest
    {
        public string Name { get; set; } = null!;
        public int Order { get; set; }
        public IFormFile? Image { get; set; }
    }
}
