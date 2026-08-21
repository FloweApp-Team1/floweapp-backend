namespace OrdersService.Infrastructure.Services
{
    public interface ICatalogServiceClient
    {
        Task<CatalogProductDto?> GetProductDetailsAsync(Guid productId, CancellationToken cancellationToken = default);
    }

    public record CatalogProductDto(Guid Id, string Name, string ImageUrl, decimal Price);
}
