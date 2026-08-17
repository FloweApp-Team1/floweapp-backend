namespace CatalogService.Domain.Entities
{
    public class Discount:CatalogBaseEntity
    {
        public Guid ProductId { get; set; }

        public decimal Percentage { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Product Product { get; set; } = default!;

    }
}
