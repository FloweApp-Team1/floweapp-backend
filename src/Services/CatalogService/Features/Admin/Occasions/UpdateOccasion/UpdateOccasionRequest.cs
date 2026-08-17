namespace CatalogService.Features.Admin.Occasions.UpdateOccasion
{
    public class UpdateOccasionRequest
    {
        public string? Name { get; set; }
        public int? Order { get; set; }
        public IFormFile? Image { get; set; }
    }
}
