namespace AddressCartService.Infrastructure.Services.Catalog
{
    public interface ICatalogClient
    {
        Task<CatalogProductDto?> GetProductByIdAsync(Guid productId, Guid? storeId = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyDictionary<Guid, CatalogProductDto>> GetProductsBatchAsync(IEnumerable<Guid> productIds, Guid? storeId = null, CancellationToken cancellationToken = default);
    }
}
